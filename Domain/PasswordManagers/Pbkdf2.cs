using System;
using ErikTheCoder.Utilities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace ErikTheCoder.Identity.Domain.PasswordManagers
{
    internal class Pbkdf2 : PasswordManagerBase
    {
        private readonly KeyDerivationPrf _keyDerivationPrf;


        public Pbkdf2(IThreadsafeRandom Random, KeyDerivationPrf KeyDerivationPrf, int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial) :
            base(Random, SaltLength, HashLength, Iterations, MinCharacters, MinLowerAlpha, MinUpperAlpha, MinDigits, MinSpecial)
        {
            _keyDerivationPrf = KeyDerivationPrf;
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
            var hashBytes = KeyDerivation.Pbkdf2(Password, saltBytes, _keyDerivationPrf, Iterations, HashLength);
            var hash = Convert.ToBase64String(hashBytes);
            return (salt, hash);
        }


        public override bool Validate(string Password, string Salt, string Hash)
        {
            var saltBytes = Convert.FromBase64String(Salt);
            var hashBytes = KeyDerivation.Pbkdf2(Password, saltBytes, _keyDerivationPrf, Iterations, HashLength);
            var hash = Convert.ToBase64String(hashBytes);
            return hash == Hash;
        }
    }
}
