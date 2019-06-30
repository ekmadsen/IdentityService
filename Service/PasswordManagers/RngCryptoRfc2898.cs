using System;
using System.Security.Cryptography;
using ErikTheCoder.Utilities;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public class RngCryptoRfc2898 : PasswordManagerBase
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
            byte[] saltBytes = new byte[SaltLength];
            Random.NextBytes(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);
            // Get derived bytes from the combined salt and password, using the specified number of iterations.
            using (Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations))
            {
                byte[] hashBytes = derivedBytes.GetBytes(HashLength);
                string hash = Convert.ToBase64String(hashBytes);
                return (salt, hash);
            }
        }


        public override bool Validate(string Password, string Salt, string Hash)
        {
            byte[] saltBytes = Convert.FromBase64String(Salt);
            int hashLength = Convert.FromBase64String(Hash).Length;
            using (Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, Iterations))
            {
                byte[] hashBytes = derivedBytes.GetBytes(hashLength);
                string hash = Convert.ToBase64String(hashBytes);
                return hash == Hash;
            }
        }
    }
}
