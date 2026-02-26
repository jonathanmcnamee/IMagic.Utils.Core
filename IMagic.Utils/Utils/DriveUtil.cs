using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

/// <summary>
/// Utility class for drive-related operations.
/// Windows-specific methods are guarded with <see cref="SupportedOSPlatformAttribute"/> and runtime checks.
/// </summary>
public static class DriveUtil
{
    [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
    [SupportedOSPlatform("windows")]
    private static extern int WNetGetUniversalName(string localPath, int infoLevel, IntPtr buffer, ref int bufferSize);

    private const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
    private const int ERROR_MORE_DATA = 234;
    private const int NO_ERROR = 0;

    /// <summary>
    /// Gets the hardware ID (PNP device ID) for a local drive or the UNC path for a network-mapped drive.
    /// Windows-only: uses WMI (<c>System.Management</c>) and WinAPI (<c>mpr.dll</c>).
    /// </summary>
    /// <param name="driveLetter">Drive letter, e.g. "C" or "C:"</param>
    /// <returns>Hardware ID string for local drives, UNC path for network drives, or <c>null</c> on failure.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown on non-Windows platforms.</exception>
    [SupportedOSPlatform("windows")]
    public static string GetHardwareIdOrNetworkPath(string driveLetter)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Drive hardware ID detection is only supported on Windows.");

        if (string.IsNullOrEmpty(driveLetter))
            throw new ArgumentException("Drive letter cannot be null or empty.", nameof(driveLetter));

        // Normalise to bare "X:" form
        driveLetter = NormalizeDriveLetter(driveLetter);

        try
        {
            string uncPath = GetUNCPath(driveLetter);
            if (!string.IsNullOrEmpty(uncPath))
                return uncPath;

            string query = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass=Win32_LogicalDiskToPartition";
            using ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject partition in partitionSearcher.Get())
            {
                string partitionDeviceId = (string)partition["DeviceID"];
                string diskQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partitionDeviceId}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition";
                using ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(diskQuery);
                foreach (ManagementObject disk in diskSearcher.Get())
                    return (string)disk["PNPDeviceID"];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error retrieving hardware ID or UNC path: " + ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Determines if a drive is removable. Cross-platform.
    /// </summary>
    public static bool IsDriveRemovable(string driveLetter)
        => GetDriveInfo(driveLetter).DriveType == DriveType.Removable;

    /// <summary>
    /// Returns a <see cref="DriveInfo"/> for the given drive letter. Cross-platform.
    /// </summary>
    public static DriveInfo GetDriveInfo(string driveLetter)
    {
        if (string.IsNullOrEmpty(driveLetter))
            throw new ArgumentException("Drive letter cannot be null or empty.", nameof(driveLetter));

        return new DriveInfo(NormalizeDriveLetter(driveLetter));
    }

    // Strips everything after the first colon so any input ("C", "C:", "C:\foo") becomes "C:".
    private static string NormalizeDriveLetter(string driveLetter)
    {
        if (!driveLetter.Contains(':'))
            driveLetter += ":";
        return driveLetter.RemoveBefore(":", false);
    }

    [SupportedOSPlatform("windows")]
    private static string GetUNCPath(string localPath)
    {
        int size = 512;
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            int result = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);
            if (result == ERROR_MORE_DATA)
            {
                Marshal.FreeHGlobal(buffer);
                buffer = Marshal.AllocHGlobal(size);
                result = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);
            }

            if (result == NO_ERROR)
                return Marshal.PtrToStringUni(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
        return null;
    }
}
