using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Utils
{
    public class Password
    {
        private static string AllowedChars = "abcdefghijklmnopqrstuwxyzABCDEFGHIJKLMNOPQRSTUWXYZ1234567890!@#$%^&*()-+_{}?[]";

        public static string Create(int length)
        {
            var password = String.Empty;
            var randomGen = new Random();

            for (int n = 0; n < length; n++)
            {
                password += AllowedChars[randomGen.Next(AllowedChars.Length - 1)];
            }

            return password;
        }
    }
}
