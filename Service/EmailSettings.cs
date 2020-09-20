using ErikTheCoder.Identity.Domain;


namespace ErikTheCoder.Identity.Service
{
    public class EmailSettings : IEmailSettings
    {
        public string From { get; set; }
        public string ConfirmationUrl { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
