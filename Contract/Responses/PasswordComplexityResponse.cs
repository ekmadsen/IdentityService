using System.Collections.Generic;


namespace ErikTheCoder.Identity.Contract.Responses
{
    public abstract class PasswordComplexityResponse
    {
        public bool PasswordValid { get; set; }
        public List<string> Messages { get; set; }
    }
}
