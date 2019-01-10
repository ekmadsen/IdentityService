using System.Collections.Generic;
using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Responses
{
    public abstract class PasswordComplexityResponse
    {
        [UsedImplicitly] public bool PasswordValid { get; set; }
        [UsedImplicitly] public List<string> Messages { get; set; }
    }
}
