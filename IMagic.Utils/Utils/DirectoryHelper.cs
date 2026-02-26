using System.IO;

public static class DirectoryHelper
{
    /// <summary>
    /// Returns the path of <paramref name="directory"/> with the drive root removed
    /// (e.g. "C:\example\folder" → "example\folder").
    /// </summary>
    public static string GetPathWithoutDrive(DirectoryInfo directory)
    {
        string fullPath = directory.FullName;
        string root = Path.GetPathRoot(fullPath);
        return fullPath.Substring(root.Length);
    }

    /// <inheritdoc cref="GetPathWithoutDrive(DirectoryInfo)"/>
    public static string GetPathWithoutDrive(string directory)
        => GetPathWithoutDrive(new DirectoryInfo(directory));

    /// <summary>
    /// Returns the volume root for <paramref name="directory"/> (e.g. "C:\").
    /// </summary>
    public static string GetVolumeName(string directory)
        => Path.GetPathRoot(directory);
}
