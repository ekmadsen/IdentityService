using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Requests
{
    [UsedImplicitly]
    public class ConfirmRequest
    {
        [UsedImplicitly] public string EmailAddress { get; set; }
        [UsedImplicitly] public string Code { get; set; }
    }
}
