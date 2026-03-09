using System.Security.Cryptography;
using Modular.Modules.Users.Helpers.Interfaces;
using System;

namespace Modular.Modules.Users.Helpers
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA512))
            {
                var key = algorithm.GetBytes(KeySize);

                return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
            }
        }

        public bool VerifyPassword(string password, string fullHash)
        {
            var parts = fullHash.Split('.');

            if (parts.Length != 3)
            {
                return false;
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using (var algorithm = new Rfc2898DeriveBytes(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA512))
            {
                var newKey = algorithm.GetBytes(KeySize);

                return CryptographicOperations.FixedTimeEquals(newKey, key);
            }
        }
    }
}
