using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using CMS.Utils.Diagnostics;

namespace CMS.Utils.Cryptography
{
    public class Sha256
    {
        /// <summary>
        /// Hashes an input string and return the hash as a hexadecimal string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSHA256Hash(string password)
        {
            SHA256Managed crypt = new SHA256Managed();         
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            StringBuilder hash = new StringBuilder();

            foreach (byte bit in crypto)
            {
                hash.Append(bit.ToString("x2"));
            }

            return hash.ToString();
        }


        /// <summary>
        /// Verifies a hash against a string.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        static public bool VerifySHA256Hash(string input, string hash)
        {
            string hashOfInput = GetSHA256Hash(input);
            Log.Debug("VerifySHA256 -> input: " + input + " generated hash: " + hashOfInput + " hash provided: " + hash); 
            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return (comparer.Compare(hashOfInput, hash) == 0);
        }
    }
}
