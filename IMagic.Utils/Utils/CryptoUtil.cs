using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SalesEssistant.Core.Utils
{
    public static class CryptoUtil
    {
        public static byte[] CalculateHash_SHA_256(byte[] input)
        {
            SHA256 shaM = new SHA256Managed();
            byte[] output = shaM.ComputeHash(input);
            return output;
        }

        public static string CalculateHash_SHA_256(string input)
        {
            byte[] outputBytes = CalculateHash_SHA_256(Encoding.Unicode.GetBytes(input));
            string output = Encoding.Unicode.GetString(outputBytes);
            return output;
        }
        public static byte[] CalculateHash_SHA_512(byte[] input)
        {
            SHA512 shaM = new SHA512Managed();
            byte[] output = shaM.ComputeHash(input);
            return output;
        }


        public static string CalculateHash_SHA_512(string input)
        {
            byte[] outputBytes = CalculateHash_SHA_512(Encoding.Unicode.GetBytes(input));
            string output = Encoding.Unicode.GetString(outputBytes);
            return output;
        }
    }
}
