using System;
using System.Collections.Generic;
using ErikTheCoder.Utilities;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public abstract class PasswordManagerBase : IPasswordManager
    {
        protected IThreadsafeRandom Random;
        protected readonly int SaltLength;
        protected readonly int HashLength;
        protected readonly int Iterations;
        private readonly int _minCharacters;
        private readonly int _minLowerAlpha;
        private readonly int _minUpperAlpha;
        private readonly int _minDigits;
        private readonly int _minSpecial;
        private bool _disposed;


        protected PasswordManagerBase(IThreadsafeRandom Random, int SaltLength, int HashLength, int Iterations, int MinCharacters, int MinLowerAlpha, int MinUpperAlpha, int MinDigits, int MinSpecial)
        {
            this.Random = Random;
            this.SaltLength = SaltLength;
            this.HashLength = HashLength;
            this.Iterations = Iterations;
            _minCharacters = MinCharacters;
            _minLowerAlpha = MinLowerAlpha;
            _minUpperAlpha = MinUpperAlpha;
            _minDigits = MinDigits;
            _minSpecial = MinSpecial;
        }


        // See Microsoft-recommended dispose pattern at https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose.
        ~PasswordManagerBase() => Dispose(false);


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void Dispose(bool Disposing)
        {
            if (_disposed) return;
            if (Disposing)
            {
                // No managed objects to free.
            }
            // Free unmanaged objects.
            Random?.Dispose();
            Random = null;
            _disposed = true;
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
