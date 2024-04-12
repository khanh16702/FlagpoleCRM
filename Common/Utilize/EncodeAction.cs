using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilize
{
    public class EncodeAction
    {
        private const int _keySize = 64; // 512 bits
        private const int _iterations = 20000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA512;

        private const char segmentDelimiter = ':';

        public static string Hash(string input, string saltStr)
        {
            byte[] salt = Encoding.UTF8.GetBytes(saltStr);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                _iterations,
                _algorithm,
                _keySize
            );
            return string.Join(
                segmentDelimiter,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                _iterations,
                _algorithm
            );
        }

        public static bool Verify(string input, string password, string salt)
        {
            var hashedInput = Hash(input, salt);
            return hashedInput.Equals(password);
        }

    }
}
