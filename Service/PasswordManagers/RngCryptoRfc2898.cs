using System;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public class RngCryptoRfc2898 : IPasswordManager
    {
        private readonly int _saltLength;
        private readonly int _hashLength;
        private readonly int _iterations;
        private readonly int _minCharacters;
        private readonly int _minLowerAlpha;
        private readonly int _minUpperAlpha;
        private readonly int _minDigits;
        private readonly int _minSpecial;



        public RngCryptoRfc2898(int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial)
        {
            _saltLength = SaltLength;
            _hashLength = HashLength;
            _iterations = Iterations;
            _minCharacters = MinCharacters;
            _minLowerAlpha = MinLowerAlpha;
            _minUpperAlpha = MinUpperAlpha;
            _minDigits = MinDigits;
            _minSpecial = MinSpecial;
        }

        
        public (string Salt, string Hash) Hash(string Password)
        {
            // Create random salt.
            // Storing a salt value with a hashed password prevents identical passwords from hashing to the same stored value.
            // See https://security.stackexchange.com/questions/17421/how-to-store-salt
            byte[] saltBytes = new byte[_saltLength];
            using (RNGCryptoServiceProvider random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(saltBytes, 0, _saltLength);
                string salt = Convert.ToBase64String(saltBytes);
                // Get derived bytes from the combined salt and password, using the specified number of iterations.
                using (Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, _iterations))
                {
                    byte[] hashBytes = derivedBytes.GetBytes(_hashLength);
                    string hash = Convert.ToBase64String(hashBytes);
                    return (salt, hash);
                }
            }
        }


        public bool Validate(string Password, string Salt, string Hash)
        {
            byte[] saltBytes = Convert.FromBase64String(Salt);
            int hashLength = Convert.FromBase64String(Hash).Length;
            using (Rfc2898DeriveBytes derivedBytes = new Rfc2898DeriveBytes(Password, saltBytes, _iterations))
            {
                byte[] hashBytes = derivedBytes.GetBytes(hashLength);
                string hash = Convert.ToBase64String(hashBytes);
                return hash == Hash;
            }
        }


        public (bool Valid, List<string> Messages) ValidateComplexity(string Password)
        {
            bool valid = true;
            List<string> messages = new List<string>();
            // Rather than concocting an unreadable regex expression, simply count characters.
            int lowerAlpha = 0;
            int upperAlpha = 0;
            int digits = 0;
            int special = 0;
            foreach (char character in Password)
            {
                // Determine character type.
                if (char.IsLower(character)) lowerAlpha++;
                else if (char.IsUpper(character)) upperAlpha++;
                else if (char.IsDigit(character)) digits++;
                else special++;
            }
            // Examine complexity.
            if (Password.Length < _minCharacters)
            {
                valid = false;
                messages.Add($"Must be at least {_minCharacters} characters.");
            }
            if (lowerAlpha < _minLowerAlpha)
            {
                valid = false;
                messages.Add($"Must include at least {_minLowerAlpha} lowercase letters.");
            }
            if (upperAlpha < _minUpperAlpha)
            {
                valid = false;
                messages.Add($"Must include at least {_minUpperAlpha} uppercase letters.");
            }
            if (digits < _minDigits)
            {
                valid = false;
                messages.Add($"Must include at least {_minDigits} digits.");
            }
            if (special < _minSpecial)
            {
                valid = false;
                messages.Add($"Must include at least {_minSpecial} special characters (non alpha or digit).");
            }
            return (valid, messages);
        }
    }
}
