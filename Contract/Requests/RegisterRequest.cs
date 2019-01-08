namespace ErikTheCoder.Identity.Contract.Requests
{
    public class RegisterRequest

    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
