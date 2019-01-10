using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Requests
{
    public class ResetPasswordRequest
    {
        [UsedImplicitly] public string EmailAddress { get; set; }
        [UsedImplicitly] public string Code { get; set; }
        [UsedImplicitly] public string NewPassword { get; set; }
    }
}
