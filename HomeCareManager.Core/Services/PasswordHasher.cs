using System;
using System.Security.Cryptography;

namespace HomeCareManager.Core.Services
{
    public static class PasswordHasher
    {
        private const string Format = "PBKDF2";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return string.Join(
                "$",
                Format,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split('$');

            if (parts.Length != 4 || !parts[0].Equals(Format, StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out int iterations))
            {
                return false;
            }

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expectedHash = Convert.FromBase64String(parts[3]);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
