using System.Collections.Generic;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public class PasswordManagerVersions : Dictionary<int, IPasswordManager>, IPasswordManagerVersions
    {
        public PasswordManagerVersions()
        {
            // TODO: Move password manager configuration to appSettings.json.
            // Add all versions.
            // ReSharper disable ArgumentsStyleLiteral
            Add(1, new RngCryptoRfc2898(
                SaltLength: 16,
                HashLength: 32,
                Iterations: 1000,
                MinCharacters: 8,
                MinLowerAlpha: 0,
                MinUpperAlpha: 0,
                MinDigits: 0,
                MinSpecial: 0
            ));
            Add(2, new RngCryptoRfc2898(
                SaltLength: 16,
                HashLength: 32,
                Iterations: 10000,
                MinCharacters: 8,
                MinLowerAlpha: 1,
                MinUpperAlpha: 1,
                MinDigits: 1,
                MinSpecial: 1
            ));
            // ReSharper restore ArgumentsStyleLiteral
        }
    }
}
