namespace ErikTheCoder.Identity.Domain
{
    public interface IEmailSettings
    {
        string From { get; set; }
        string ConfirmationUrl { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        bool EnableSsl { get; set; }
        string Username { get; set; }
        string Password { get; set; }
    }
}
