using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public static class IOUtil
{
    #region Text/Xml Files

    public static void WriteTextFile(string fileName, string contents, bool append = false, bool writeLine = true)
    {
        using StreamWriter sw = new StreamWriter(fileName, append);
        if (writeLine)
            sw.WriteLine(contents);
        else
            sw.Write(contents);
    }
    public static void WriteXmlFile(string fileName, XmlDocument xml)
    {
        using XmlTextWriter writer = new XmlTextWriter(fileName, null);
        writer.Formatting = Formatting.Indented;
        xml.Save(writer);
    }

    public static string ReadTextFile(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        using FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using StreamReader sr = new StreamReader(file);
        return sr.ReadToEnd();
    }

    #endregion
    #region Binary Files
    public static byte[] ReadBinaryFile(string fileName)
    {
        if (!File.Exists(fileName))
            return null;

        return File.ReadAllBytes(fileName);
    }

    public static void WriteBinaryFile(string fileName, byte[] binary)
    {
        using FileStream fs = new FileStream(fileName, FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(binary);
    }
    #endregion
    #region byte arrays

    public static string ByteArrayToString(Encoding encoding, byte[] byteArray)
    {
        return encoding.GetString(byteArray);
    }
    public static byte[] StringToByteArray(string s) => Encoding.ASCII.GetBytes(s);

    public static string ByteArrayToString(byte[] bytes) => Encoding.ASCII.GetString(bytes);
    #endregion
    #region Base 64 encode/decode
    public static string Base64Encode(string str)
    {
        byte[] encbuff = Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(encbuff);
    }

    public static string Base64Decode(string str)
    {
        byte[] decbuff = Convert.FromBase64String(str);
        return Encoding.UTF8.GetString(decbuff);
    }
    #endregion


    #region Directory Methods

    /// <summary>
    /// Returns true if the directory contains any file matching one of the given extensions (e.g. "*.jpg").
    /// </summary>
    public static bool HasImageFiles(string directory, List<string> imageExtensions)
    {
        try
        {
            foreach (string pattern in imageExtensions)
            {
                if (Directory.GetFiles(directory, pattern, SearchOption.AllDirectories).Length > 0)
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Counts all files matching the given extensions under <paramref name="directory"/>.
    /// </summary>
    public static int GetFileCount(string directory, List<string> fileExtensions, bool searchSubDirectories = true)
    {
        try
        {
            SearchOption searchOption = searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            int fileCount = 0;
            foreach (string extension in fileExtensions)
            {
                string pattern = extension.StartsWith(".") ? $"*{extension}" : $"*.{extension}";
                fileCount += Directory.GetFiles(directory, pattern, searchOption).Length;
            }
            return fileCount;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Returns the file names (not full paths) matching the given extensions under <paramref name="directory"/>.
    /// </summary>
    public static List<string> GetFiles(string directory, List<string> fileExtensions, bool searchSubDirectories = true)
    {
        List<string> output = new List<string>();
        try
        {
            SearchOption searchOption = searchSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (string extension in fileExtensions)
            {
                string pattern = extension.StartsWith(".") ? $"*{extension}" : $"*.{extension}";
                foreach (string file in Directory.GetFiles(directory, pattern, searchOption))
                    output.Add(Path.GetFileName(file));
            }
            return output;
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Recursively sums the sizes of all files under <paramref name="rootPath"/>.
    /// </summary>
    public static long CalculateDirectorySize(string rootPath)
    {
        long total = 0;
        try
        {
            foreach (string file in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                try { total += new FileInfo(file).Length; }
                catch { }
            }
        }
        catch { }
        return total;
    }

    /// <summary>
    /// Creates each directory in <paramref name="paths"/> if it does not already exist.
    /// </summary>
    public static void CreateDirectoryIfNotExists(params string[] paths)
        => CreateDirectoryIfNotExists(paths.ToList());

    /// <inheritdoc cref="CreateDirectoryIfNotExists(string[])"/>
    public static void CreateDirectoryIfNotExists(List<string> paths)
    {
        foreach (string path in paths)
            CreateDirectoryIfNotExists(path);
    }

    /// <inheritdoc cref="CreateDirectoryIfNotExists(string[])"/>
    public static void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    #endregion

    #region Utility Methods
    private static readonly string[] ByteOrders = ["GB", "MB", "KB", "Bytes"];

    public static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        long max = (long)Math.Pow(scale, ByteOrders.Length - 1);

        foreach (string order in ByteOrders)
        {
            if (bytes > max)
                return $"{decimal.Divide(bytes, max):##.##} {order}";
            max /= scale;
        }
        return "0 Bytes";
    }
    //public static string GetMimeType(string fileName)
    //{
    //    string mimeType = "application/unknown";

    //    string ext = Path.GetExtension(fileName).ToLower();

    //    using (RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext))
    //    {
    //        if (regKey != null && regKey.GetValue("Content Type") != null)
    //        {
    //            mimeType = regKey.GetValue("Content Type").ToString();
    //        }
    //    }

    //    return mimeType;
    //}
    private const string AllowedFileNameChars = "abcdefghijkmnopqrstuvwxyz0123456789";

    public static string GenerateRandomFileName(int length)
    {
        char[] chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = AllowedFileNameChars[Random.Shared.Next(0, AllowedFileNameChars.Length)];
        return new string(chars);
    }
    #endregion
}
