namespace ErikTheCoder.Identity.Contract.Requests
{
    public class ResetPasswordRequest
    {
        public string EmailAddress { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
