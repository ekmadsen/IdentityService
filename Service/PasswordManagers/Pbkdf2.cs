using System;
using ErikTheCoder.ServiceContract;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public class Pbkdf2 : PasswordManagerBase
    {
        private readonly KeyDerivationPrf _keyDerivationPrf;


        public Pbkdf2(ISafeRandom SafeRandom, KeyDerivationPrf KeyDerivationPrf, int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial) :
            base(SafeRandom, SaltLength, HashLength, Iterations, MinCharacters, MinLowerAlpha, MinUpperAlpha, MinDigits, MinSpecial)
        {
            _keyDerivationPrf = KeyDerivationPrf;
        }


        public override (string Salt, string Hash) Hash(string Password)
        {
            // Create random salt.
            // Storing a salt value with a hashed password prevents identical passwords from hashing to the same stored value.
            // See https://security.stackexchange.com/questions/17421/how-to-store-salt
            byte[] saltBytes = new byte[SaltLength];
            SafeRandom.NextBytes(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);
            // Get derived bytes from the combined salt and password, using the specified number of iterations.
            byte[] hashBytes = KeyDerivation.Pbkdf2(Password, saltBytes, _keyDerivationPrf, Iterations, HashLength);
            string hash = Convert.ToBase64String(hashBytes);
            return (salt, hash);
        }


        public override bool Validate(string Password, string Salt, string Hash)
        {
            byte[] saltBytes = Convert.FromBase64String(Salt);
            byte[] hashBytes = KeyDerivation.Pbkdf2(Password, saltBytes, _keyDerivationPrf, Iterations, HashLength);
            string hash = Convert.ToBase64String(hashBytes);
            return hash == Hash;
        }
    }
}
