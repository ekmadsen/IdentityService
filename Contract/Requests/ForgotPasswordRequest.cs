using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Contract.Requests
{
    public class ForgotPasswordRequest
    {
        [UsedImplicitly]
        public string EmailAddress { get; set; }
    }
}
