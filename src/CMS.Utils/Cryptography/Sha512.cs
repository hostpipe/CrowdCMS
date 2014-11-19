using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CMS.Utils.Cryptography
{
    public class Sha512
    {
        /// <summary>
        /// Hashes an input string and return the hash as a hexadecimal string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static public string GetSHA512Hash(string input)
        {
            SHA512 hasher = SHA512.Create();
            byte[] data = hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        /// <summary>
        /// Verifies a hash against a string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        static public bool VerifySHA512Hash(string input, string hash)
        {
            string hashOfInput = GetSHA512Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return (comparer.Compare(hashOfInput, hash) == 0);
        }
    }
}
