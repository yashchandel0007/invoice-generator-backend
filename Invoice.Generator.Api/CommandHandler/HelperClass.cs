using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Invoice.Generator.Api.CommandHandler
{
    public static class HelperClass 
    {
        public static byte[] GetPasswordHash(string password, byte[] salt, int iterations)
        {
            Rfc2898DeriveBytes hash = new Rfc2898DeriveBytes(password, salt, iterations);
            return hash.GetBytes(24);
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[10];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random value.
                rngCsp.GetBytes(salt);
            }
            return salt;
        }
        public static bool PasswordMatch(string password, string currentPasswordHash, string salt, int iterations)
        {
            var tempHash = HelperClass.GetPasswordHash(password, Convert.FromBase64String(salt), iterations);

            if (String.Compare(currentPasswordHash, Convert.ToBase64String(tempHash)) == 0)
                return true;

            return false;
        }
    }
}
