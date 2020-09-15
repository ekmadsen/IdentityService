using System;
using System.Security.Cryptography;
using ErikTheCoder.Utilities;


namespace ErikTheCoder.Identity.Domain.PasswordManagers
{
    internal class RngCryptoRfc2898 : PasswordManagerBase
    {
        public RngCryptoRfc2898(IThreadsafeRandom Random, int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial) :
            base(Random, SaltLength, HashLength, Iterations, MinCharacters, MinLowerAlpha, MinUpperAlpha, MinDigits, MinSpecial)
        {
        }


        public override (string Salt, string Hash) Hash(string Password)
        {
            // Create random salt.
            // Storing a salt value with a hashed password prevents identical passwords from hashing to the same stored value.
            // See https://security.stackexchange.com/questions/17421/how-to-store-salt
            var saltBytes = new byte[SaltLength];
            Random.NextBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);
            // Get derived bytes from the combined salt and password, using the specified number of iterations.
            using (var derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations))
            {
                var hashBytes = derivedBytes.GetBytes(HashLength);
                var hash = Convert.ToBase64String(hashBytes);
                return (salt, hash);
            }
        }


        public override bool Validate(string Password, string Salt, string Hash)
        {
            var saltBytes = Convert.FromBase64String(Salt);
            var hashLength = Convert.FromBase64String(Hash).Length;
            using (var derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations))
            {
                var hashBytes = derivedBytes.GetBytes(hashLength);
                var hash = Convert.ToBase64String(hashBytes);
                return hash == Hash;
            }
        }
    }
}
