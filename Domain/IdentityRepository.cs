using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Dapper;
using ErikTheCoder.Data;
using ErikTheCoder.Domain;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using ErikTheCoder.Identity.Domain.PasswordManagers;
using ErikTheCoder.Logging;
using ErikTheCoder.ServiceContract;


namespace ErikTheCoder.Identity.Domain
{
    internal class IdentityRepository : RepositoryBase, IIdentityRepository
    {
        private const int _passwordManagerVersion = 3;
        private readonly IdentityRecordFactory _factory;
        private readonly PasswordManagerVersions _passwordManagerVersions;
        private readonly IEmailSettings _emailSettings;


        public IdentityRepository(ILogger Logger, ICorrelationIdAccessor CorrelationIdAccessor, ILoggedDatabase Database, IdentityRecordFactory Factory, PasswordManagerVersions PasswordManagerVersions, IEmailSettings EmailSettings) :
            base(Logger, CorrelationIdAccessor, Database)
        {
            _factory = Factory;
            _passwordManagerVersions = PasswordManagerVersions;
            _emailSettings = EmailSettings;
        }


        public async Task<User> LoginAsync(LoginRequest Request)
        {
            // Validate password against hash stored in database.
            const string query = @"
                select u.id, u.Username, u.PasswordManagerVersion, u.Salt, u.PasswordHash, u.EmailAddress, u.FirstName, u.LastName
                from [Identity].Users u
                where u.Username = @username
                and u.Confirmed = 1
                and u.Enabled = 1";
            var (connection, transaction, disposeDbResources) = await GetDbResourcesAsync();
            try
            {
                var userRecord = await connection.QuerySingleOrDefaultAsync<UserRecord>(query, Request);
                if (userRecord == null)
                {
                    Logger.Log(CorrelationId, $"{Request.Username} user not found.");
                    return null;
                }
                var passwordManager = _passwordManagerVersions[userRecord.PasswordManagerVersion];
                if (!passwordManager.Validate(Request.Password, userRecord.Salt, userRecord.PasswordHash))
                {
                    // Password is invalid.
                    Logger.Log(CorrelationId,
                        $"{Request.Username} user not authenticated.  Invalid username or password.");
                    return null;
                }
                // Password is valid.
                Logger.Log(CorrelationId, $"{Request.Username} user authenticated.");
                // Add roles and claims.
                var user = _factory.CreateUser(userRecord);
                // TODO: Execute roles and claims queries in one round-trip to SQL Server via Dapper's QueryMultiple method.
                await AddRolesAsync(connection, user);
                await AddClaimsAsync(connection, user);
                return user;
            }
            catch
            {
                transaction?.TryRollback();
                throw;
            }
            finally
            {
                if (disposeDbResources)
                {
                    connection.Dispose();
                    transaction?.Dispose();
                }
            }
        }


        private async Task AddRolesAsync(IDbConnection Connection, User User)
        {
            Logger.Log(CorrelationId, $"Adding roles to {User.Username} User object.");
            const string query = @"
                select r.Name
                from [Identity].UserRoles ur
                inner join [Identity].Users u on ur.UserId = u.Id
                inner join [Identity].Roles r on ur.RoleId = r.Id
                where u.Id = @id
                order by r.Name asc";
            var roles = await Connection.QueryAsync<RoleRecord>(query, User);
            foreach (var role in roles)
            {
                Logger.Log(CorrelationId, $"{role.Name} role");
                User.Roles.Add(role.Name);
            }
        }


        private async Task AddClaimsAsync(IDbConnection Connection, User User)
        {
            Logger.Log(CorrelationId, $"Adding claims to {User.Username} User object.");
            const string query = @"
                select c.[Type], uc.[Value]
                from [Identity].UserClaims uc
                inner join [Identity].Users u on uc.UserId = u.Id
                inner join [Identity].Claims c on uc.ClaimId = c.Id
                where u.Id = @id
                order by c.[Type] asc, uc.[Value] asc";
            var claims = await Connection.QueryAsync<ClaimRecord>(query, User);
            foreach (var claim in claims)
            {
                Logger.Log(CorrelationId, $"Type = {claim.Type}, Value = {claim.Value} claim");
                if (!User.Claims.ContainsKey(claim.Type))
                    User.Claims.Add(claim.Type, new HashSet<string>(StringComparer.CurrentCultureIgnoreCase));
                User.Claims[claim.Type].Add(claim.Value);
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest Request)
        {
            // TODO: Validate email address and username are available.
            // Validate password meets complexity requirements.
            var passwordManager = _passwordManagerVersions[_passwordManagerVersion];
            var (valid, messages) = passwordManager.ValidateComplexity(Request.Password);
            if (!valid)
            {
                return new RegisterResponse
                {
                    PasswordValid = false,
                    Messages = messages
                };
            }
            string code;
            var (connection, transaction, disposeDbResources) = await GetDbResourcesAsync();
            try
            {
                // Add user to database.
                var (salt, passwordHash) = passwordManager.Hash(Request.Password);
                var addUserQueryParameters = new
                {
                    Request.Username,
                    PasswordManagerVersion = _passwordManagerVersion,
                    salt,
                    passwordHash,
                    Request.EmailAddress,
                    Request.FirstName,
                    Request.LastName
                };
                const string addUserQuery = @"
                insert into [Identity].Users (Username, Enabled, Confirmed, PasswordManagerVersion, Salt, PasswordHash, EmailAddress, FirstName, LastName)
                output inserted.id
                values (@username, 1, 0, @passwordManagerVersion, @salt, @passwordHash, @emailAddress, @firstName, @lastName)";
                code = Guid.NewGuid().ToString();
                var userId = (int) await connection.ExecuteScalarAsync(addUserQuery, addUserQueryParameters);
                // Add confirmation to database.
                var confirmationParam = new
                {
                    userId,
                    Request.EmailAddress,
                    code,
                    Sent = DateTime.Now
                };
                const string confirmationQuery = @"
                insert into [Identity].UserConfirmations (UserId, EmailAddress, Code, Sent)
                values (@userId, @emailAddress, @code, @sent)";
                await connection.ExecuteAsync(confirmationQuery, confirmationParam);
                transaction?.TryCommit();
            }
            catch
            {
                transaction?.TryRollback();
                throw;
            }
            finally
            {
                if (disposeDbResources)
                {
                    connection.Dispose();
                    transaction?.Dispose();
                }
            }
            // Send confirmation email.
            using (var email = new MailMessage())
            {
                // TODO: Upgrade confirmation message from plain text to HTML.
                // TODO: Move confirmation message text to database.
                email.From = new MailAddress(_emailSettings.From);
                email.To.Add(new MailAddress(Request.EmailAddress));
                email.Subject = "account activation";
                // TODO: Use MVC routing to create email confirmation hyperlink.
                var confirmationUrl = string.Format(_emailSettings.ConfirmationUrl, Request.EmailAddress, code);
                email.Body = $"Click {confirmationUrl} to confirm your email address.";
                using (var smtpClient = new SmtpClient
                {
                    Host = _emailSettings.Host,
                    Port = _emailSettings.Port,
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
                })
                {
                    smtpClient.Send(email);
                }
            }
            return new RegisterResponse
            {
                PasswordValid = true,
                Messages = messages
            };
        }
    }
}
