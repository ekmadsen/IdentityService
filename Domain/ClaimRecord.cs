using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Domain
{
    internal class ClaimRecord
    {
        [UsedImplicitly] public string Type;
        [UsedImplicitly] public string Value;
    }
}