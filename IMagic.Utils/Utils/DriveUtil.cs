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

    // Layout returned by WNetGetUniversalName when UNIVERSAL_NAME_INFO_LEVEL is used:
    // the first (and only) field is a pointer to the UNC string inside the same buffer.
    [StructLayout(LayoutKind.Sequential)]
    private struct UniversalNameInfo
    {
        public IntPtr UniversalName;
    }

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
        IntPtr buffer = IntPtr.Zero;
        try
        {
            buffer = Marshal.AllocHGlobal(size);
            int result = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);
            if (result == ERROR_MORE_DATA)
            {
                // On ERROR_MORE_DATA the API reports the required size in bytes.
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
                buffer = Marshal.AllocHGlobal(size);
                result = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);
            }

            if (result == NO_ERROR)
                return ReadUniversalName(buffer);
        }
        finally
        {
            if (buffer != IntPtr.Zero)
                Marshal.FreeHGlobal(buffer);
        }
        return null;
    }

    // Pure marshalling - no OS dependency, kept internal so the struct layout can be unit tested.
    internal static string ReadUniversalName(IntPtr buffer)
    {
        if (buffer == IntPtr.Zero)
            return null;

        IntPtr namePtr = Marshal.PtrToStructure<UniversalNameInfo>(buffer).UniversalName;
        return namePtr == IntPtr.Zero ? null : Marshal.PtrToStringUni(namePtr);
    }
}
