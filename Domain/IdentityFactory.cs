using ErikTheCoder.ServiceContract;


namespace ErikTheCoder.Identity.Domain
{
    internal class IdentityFactory : IIdentityFactory
    {
        public virtual User CreateUser() => new User();
        public virtual IEmailSettings CreateEmailSettings() => new EmailSettings();
    }
}
