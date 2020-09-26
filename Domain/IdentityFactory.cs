using ErikTheCoder.Data;
using ErikTheCoder.Identity.Domain.PasswordManagers;
using ErikTheCoder.Logging;
using ErikTheCoder.ServiceContract;
using ErikTheCoder.Utilities;


namespace ErikTheCoder.Identity.Domain
{
    public class IdentityFactory : IIdentityFactory
    {
        private readonly ILogger _logger;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;
        private readonly ILoggedDatabase _database;
        private readonly IEmailSettings _emailSettings;
        private readonly PasswordManagerVersions _passwordManagerVersions;


        public IdentityFactory(ILogger Logger, ICorrelationIdAccessor CorrelationIdAccessor, ILoggedDatabase Database, IThreadsafeRandom ThreadsafeRandom, IEmailSettings EmailSettings)
        {
            _logger = Logger;
            _correlationIdAccessor = CorrelationIdAccessor;
            _database = Database;
            _emailSettings = EmailSettings;
            _passwordManagerVersions = new PasswordManagerVersions(ThreadsafeRandom);
        }


        // Create domain classes.
        public virtual User CreateUser() => new User();


        internal User CreateUser(UserRecord Record)
        {
            var user = CreateUser();
            user.EmailAddress = Record.EmailAddress;
            user.FirstName = Record.FirstName;
            user.Id = Record.Id;
            user.LastName = Record.LastName;
            user.PasswordHash = Record.PasswordHash;
            user.PasswordManagerVersion = Record.PasswordManagerVersion;
            user.Salt = Record.Salt;
            user.Username = Record.Username;
            return user;
        }


        // Create repository classes.
        public virtual IIdentityRepository CreateIdentityRepository() => new IdentityRepository(_logger, _correlationIdAccessor, _database, this, _passwordManagerVersions, _emailSettings);
    }
}
