using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Service.Entities
{
    [UsedImplicitly]
    public class Claim
    {
        public string Type { get; [UsedImplicitly] set; }
        public string Value { get; [UsedImplicitly] set; }
    }
}
