using System.Collections.Generic;
using ErikTheCoder.ServiceContract;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public abstract class PasswordManagerBase : IPasswordManager
    {
        protected readonly ISafeRandom SafeRandom;
        protected readonly int SaltLength;
        protected readonly int HashLength;
        protected readonly int Iterations;
        private readonly int _minCharacters;
        private readonly int _minLowerAlpha;
        private readonly int _minUpperAlpha;
        private readonly int _minDigits;
        private readonly int _minSpecial;


        protected PasswordManagerBase(ISafeRandom SafeRandom, int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial)
        {
            this.SafeRandom = SafeRandom;
            this.SaltLength = SaltLength;
            this.HashLength = HashLength;
            this.Iterations = Iterations;
            _minCharacters = MinCharacters;
            _minLowerAlpha = MinLowerAlpha;
            _minUpperAlpha = MinUpperAlpha;
            _minDigits = MinDigits;
            _minSpecial = MinSpecial;
        }


        public abstract (string Salt, string Hash) Hash(string Password);


        public abstract bool Validate(string Password, string Salt, string Hash);


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
