using ErikTheCoder.Data;
using ErikTheCoder.Logging;
using ErikTheCoder.ServiceContract;
using ErikTheCoder.Utilities;


namespace ErikTheCoder.Identity.Domain
{
    internal class IdentityRecordFactory : IdentityFactory
    {
        public IdentityRecordFactory(ILogger Logger, ICorrelationIdAccessor CorrelationIdAccessor, ILoggedDatabase Database, IThreadsafeRandom ThreadsafeRandom, IEmailSettings EmailSettings) :
            base(Logger, CorrelationIdAccessor, Database, ThreadsafeRandom, EmailSettings)
        {
        }


        public User CreateUser(UserRecord Record)
        {
            var user = CreateUser();
            user.FirstName = Record.FirstName;
            user.EmailAddress = Record.EmailAddress;
            user.Id = Record.Id;
            user.LastName = Record.LastName;
            user.PasswordHash = Record.PasswordHash;
            user.PasswordManagerVersion = Record.PasswordManagerVersion;
            user.Salt = Record.Salt;
            return user;
        }
    }
}
