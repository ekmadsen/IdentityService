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
        private readonly IdentityRecordFactory _recordFactory;
        private readonly PasswordManagerVersions _passwordManagerVersions;


        public IdentityFactory(ILogger Logger, ICorrelationIdAccessor CorrelationIdAccessor, ILoggedDatabase Database, IThreadsafeRandom ThreadsafeRandom, IEmailSettings EmailSettings)
        {
            _logger = Logger;
            _correlationIdAccessor = CorrelationIdAccessor;
            _database = Database;
            _emailSettings = EmailSettings;
            _recordFactory = new IdentityRecordFactory(_logger, _correlationIdAccessor, _database, ThreadsafeRandom, _emailSettings);
            _passwordManagerVersions = new PasswordManagerVersions(ThreadsafeRandom);
        }


        // Create domain classes.
        public virtual User CreateUser() => new User();


        // Create repository classes.
        public virtual IIdentityRepository CreateIdentityRepository() => new IdentityRepository(_logger, _correlationIdAccessor, _database, _recordFactory, _passwordManagerVersions, _emailSettings);
    }
}
