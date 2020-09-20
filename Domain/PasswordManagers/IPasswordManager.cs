using System;
using System.Collections.Generic;


namespace ErikTheCoder.Identity.Domain.PasswordManagers
{
    internal interface IPasswordManager : IDisposable
    {
        (string Salt, string Hash) Hash(string Password);
        bool Validate(string Password, string Salt, string Hash);
        (bool Valid, List<string> Messages) ValidateComplexity(string Password);
    }
}
