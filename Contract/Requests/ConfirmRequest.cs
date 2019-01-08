namespace ErikTheCoder.Identity.Contract.Requests
{
    public class ConfirmRequest
    {
        public string EmailAddress { get; set; }
        public string Code { get; set; }
    }
}
