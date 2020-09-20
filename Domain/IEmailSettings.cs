using JetBrains.Annotations;


namespace ErikTheCoder.Identity.Domain
{
    public interface IEmailSettings
    {
        string From { get; [UsedImplicitly] set; }
        string ConfirmationUrl { get; [UsedImplicitly] set; }
        string Host { get; [UsedImplicitly] set; }
        int Port { get; [UsedImplicitly] set; }
        bool EnableSsl { get; [UsedImplicitly] set; }
        string Username { get; [UsedImplicitly] set; }
        string Password { get; [UsedImplicitly] set; }
    }
}
