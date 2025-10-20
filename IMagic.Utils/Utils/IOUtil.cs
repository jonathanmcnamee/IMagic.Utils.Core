using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using Microsoft.Win32;

public static class IOUtil
{
    #region Text/Xml Files

    public static void WriteTextFile(string fileName, string contents, bool append = false, bool writeLine = true)
    {
        try
        {
            using (StreamWriter sw = new StreamWriter(fileName, append))
            {
                if (writeLine)
                {
                    sw.WriteLine(contents);
                }
                else
                {
                    sw.Write(contents);
                }
            }
        }
        catch (Exception) { throw; }
    }
    public static void WriteXmlFile(string fileName, XmlDocument xml)
    {
        XmlTextWriter writer = new XmlTextWriter(fileName, null);
        writer.Formatting = Formatting.Indented;
        xml.Save(writer);
    }

    public static string ReadTextFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch (Exception) { throw; }
        }
        return null;
    }

    #endregion
    #region Binary Files
    public static byte[] ReadBinaryFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        return br.ReadBytes((int)fs.Length);
                    }
                }
            }
            catch (Exception) { throw; }
        }
        return null;
    }

    public static void WriteBinaryFile(string fileName, byte[] binary)
    {
        try
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(binary);
                }
            }
        }
        catch (Exception) { throw; }
    }
    #endregion
    #region byte arrays

    public static string ByteArrayToString(Encoding encoding, byte[] byteArray)
    {
        return encoding.GetString(byteArray);
    }
    public static byte[] StringToByteArray(string s)
    {
        System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        Byte[] bytes = encoding.GetBytes(s);
        return bytes;
    }

    public static string ByteArrayToString(byte[] bytes)
    {
        System.Text.Encoding enc = System.Text.Encoding.ASCII;
        string myString = enc.GetString(bytes);
        return myString;
    }
    #endregion
    #region Base 64 encode/decode
    public static string base64Encode(string str)
    {
        byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
        return Convert.ToBase64String(encbuff);
    }

    public static string base64Decode(string str)
    {
        byte[] decbuff = Convert.FromBase64String(str);
        return System.Text.Encoding.UTF8.GetString(decbuff);
    }
    #endregion


    #region Utility Methods
    public static string FormatBytes(long bytes)
    {
        const int scale = 1024;
        string[] orders = new string[] { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
            {
                return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
            }
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
    public static string GenerateRandomFileName(int length)
    {
        string allowedChars = "abcdefghijkmnopqrstuvwxyz0123456789";

        char[] chars = new char[length];

        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            chars[i] = allowedChars[random.Next(0, allowedChars.Length)];
        }

        return new string(chars);
    }
    #endregion
}
