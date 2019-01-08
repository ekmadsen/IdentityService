using System.Collections.Generic;


namespace ErikTheCoder.Identity.Service.PasswordManagers
{
    public interface IPasswordManager
    {
        (string Salt, string Hash) Hash(string Password);
        bool Validate(string Password, string Salt, string Hash);
        (bool Valid, List<string> Messages) ValidateComplexity(string Password);
    }
}
