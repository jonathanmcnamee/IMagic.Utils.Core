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
            using (SHA256 shaM = SHA256.Create())
            {
                byte[] output = shaM.ComputeHash(input);
                return output;
            }
        }

        public static string CalculateHash_SHA_256(string input)
        {
            byte[] outputBytes = CalculateHash_SHA_256(Encoding.UTF8.GetBytes(input));
            string output = Convert.ToBase64String(outputBytes);
            return output;
        }
        public static byte[] CalculateHash_SHA_512(byte[] input)
        {
            using (SHA512 shaM = SHA512.Create())
            {
                byte[] output = shaM.ComputeHash(input);
                return output;
            }
        }


        public static string CalculateHash_SHA_512(string input)
        {
            byte[] outputBytes = CalculateHash_SHA_512(Encoding.UTF8.GetBytes(input));
            string output = Convert.ToBase64String(outputBytes);
            return output;
        }
    }
}
