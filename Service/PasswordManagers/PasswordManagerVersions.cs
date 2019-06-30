using System.Collections.Generic;
using ErikTheCoder.Utilities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public class PasswordManagerVersions : Dictionary<int, IPasswordManager>, IPasswordManagerVersions
    {
        public PasswordManagerVersions(IThreadsafeRandom Random)
        {
            // Add all versions.
            // ReSharper disable ArgumentsStyleLiteral
            // ReSharper disable ArgumentsStyleNamedExpression
            Add(1, new RngCryptoRfc2898(
                Random: Random,
                SaltLength: 16,
                HashLength: 32,
                Iterations: 1_000,
                MinCharacters: 8,
                MinLowerAlpha: 0,
                MinUpperAlpha: 0,
                MinDigits: 0,
                MinSpecial: 0
            ));
            Add(2, new RngCryptoRfc2898(
                Random: Random,
                SaltLength: 16,
                HashLength: 32,
                Iterations: 10_000,
                MinCharacters: 8,
                MinLowerAlpha: 1,
                MinUpperAlpha: 1,
                MinDigits: 1,
                MinSpecial: 1
            ));
            Add(3, new Pbkdf2(
                Random: Random,
                KeyDerivationPrf: KeyDerivationPrf.HMACSHA512,
                SaltLength: 16,
                HashLength: 32,
                Iterations: 16_384,
                MinCharacters: 8,
                MinLowerAlpha: 1,
                MinUpperAlpha: 1,
                MinDigits: 1,
                MinSpecial: 1
            ));
            // ReSharper restore ArgumentsStyleLiteral
            // ReSharper restore ArgumentsStyleNamedExpression
        }
    }
}
