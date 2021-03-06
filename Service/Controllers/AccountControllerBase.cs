﻿using System.Threading.Tasks;
using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Identity.Contract;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using ErikTheCoder.Identity.Domain;
using ErikTheCoder.Logging;
using ErikTheCoder.ServiceContract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControllerBase = ErikTheCoder.AspNetCore.Middleware.ControllerBase;


namespace ErikTheCoder.Identity.Service.Controllers
{
    [Authorize(Policy = Policy.Admin)]
    [Route("account")]
    public abstract class AccountControllerBase : ControllerBase, IAccountService
    {
        private readonly IIdentityRepository _identityRepository;


        protected AccountControllerBase(IAppSettings AppSettings, ILogger Logger, IIdentityRepository IdentityRepository) :
            base(AppSettings, Logger)
        {
            _identityRepository = IdentityRepository;
        }


        // Access ASP.NETCore's Request property via this.Request.
        // ReSharper disable ParameterHidesMember
        [HttpPost("login")]
        public virtual async Task<User> LoginAsync([FromBody] LoginRequest Request) => await _identityRepository.LoginAsync(Request);


        [HttpPost("register")]
        public virtual async Task<RegisterResponse> RegisterAsync([FromBody] RegisterRequest Request) => await _identityRepository.RegisterAsync(Request);


        //[HttpPost("confirm")]
        //public virtual async Task ConfirmAsync([FromBody] ConfirmRequest Request)
        //{
        //    // TODO: Run multiple SQL update statements in a transaction.  Rollback if an exception occurs.
        //    // Update confirmation in database.
        //    var confirmationQueryParameters = new
        //    {
        //        Request.EmailAddress,
        //        Request.Code,
        //        Received = DateTime.Now
        //    };
        //    // TODO: Add expiration for confirmation code.
        //    const string confirmationQuery = @"
        //        declare @userId int
        //        update [Identity].UserConfirmations
        //        set Received = @received, @userId = UserId
        //        where EmailAddress = @emailAddress
        //        and Code = @code
        //        select @userId";
        //    using (IDbConnection connection = await _database.OpenConnectionAsync(CorrelationId))
        //    {
        //        var userId = (int)await connection.ExecuteScalarAsync(confirmationQuery, confirmationQueryParameters);
        //        if (userId > 0)
        //        {
        //            // Update user in database.
        //            var userQueryParameters = new { userId };
        //            const string userQuery = @"
        //                update [Identity].Users
        //                set Confirmed = 1
        //                where Id = @userId";
        //            await connection.ExecuteAsync(userQuery, userQueryParameters);
        //        }
        //        // TODO: Add invalid or expired code message.
        //    }
        //}


        //[HttpPost("forgotpassword")]
        //public virtual async Task ForgotPasswordAsync([FromBody] ForgotPasswordRequest Request)
        //{
        //    // TODO: Validate email address but don't disclose to user whether an account exists for the email address.
        //    // TODO: Include username in email message.
        //    // Add confirmation to database.
        //    var code = Guid.NewGuid().ToString();
        //    var confirmationQueryParameters = new
        //    {
        //        Request.EmailAddress,
        //        code,
        //        Sent = DateTime.Now
        //    };
        //    const string confirmationQuery = @"
        //        declare @userId int
        //        select @userId = u.Id
        //        from [Identity].Users u
        //        where u.EmailAddress = @emailAddress
        //        insert into [Identity].UserConfirmations (UserId, EmailAddress, Code, Sent)
        //        values (@userId, @emailAddress, @code, @sent)";
        //    using (IDbConnection connection = await _database.OpenConnectionAsync(CorrelationId))
        //    {
        //        await connection.ExecuteAsync(confirmationQuery, confirmationQueryParameters);
        //    }
        //    // Send confirmation email.
        //    using (var email = new MailMessage())
        //    {
        //        // TODO: Upgrade confirmation message from plain text to HTML.
        //        // TODO: Move confirmation message text to database.
        //        email.From = new MailAddress(AppSettings.Email.From);
        //        email.To.Add(new MailAddress(Request.EmailAddress));
        //        email.Subject = "password reset";
        //        // TODO: Use MVC routing to create email confirmation hyperlink.
        //        var resetUrl = string.Format(AppSettings.Account.ResetUrl, Request.EmailAddress, code);
        //        email.Body = $"Click {resetUrl} to reset your password.";
        //        using (var smtpClient = new SmtpClient
        //        {
        //            Host = AppSettings.Email.Host,
        //            Port = AppSettings.Email.Port,
        //            EnableSsl = AppSettings.Email.EnableSsl,
        //            Credentials = new NetworkCredential(AppSettings.Email.Username, AppSettings.Email.Password)
        //        })
        //        {
        //            await smtpClient.SendMailAsync(email);
        //        }
        //    }
        //}


        //[HttpPost("resetpassword")]
        //public virtual async Task<ResetPasswordResponse> ResetPasswordAsync([FromBody] ResetPasswordRequest Request)
        //{
        //    // TODO: Run multiple SQL update statements in a transaction.  Rollback if an exception occurs.
        //    // Validate password meets complexity requirements.
        //    var passwordManager = _passwordManagerVersions[_passwordManagerVersion];
        //    var (valid, messages) = passwordManager.ValidateComplexity(Request.NewPassword);
        //    if (!valid)
        //    {
        //        return new ResetPasswordResponse
        //        {
        //            PasswordValid = false,
        //            Messages = messages
        //        };
        //    }
        //    var (salt, passwordHash) = passwordManager.Hash(Request.NewPassword);
        //    // Update confirmation in database.
        //    var confirmationQueryParameters = new
        //    {
        //        Request.EmailAddress,
        //        Request.Code,
        //        Received = DateTime.Now
        //    };
        //    // TODO: Add expiration for reset code.
        //    const string confirmationQuery = @"
        //        declare @userId int
        //        update [Identity].UserConfirmations
        //        set Received = @received, @userId = UserId
        //        where EmailAddress = @emailAddress
        //        and Code = @code
        //        select @userId";
        //    using (IDbConnection connection = await _database.OpenConnectionAsync(CorrelationId))
        //    {
        //        var userId = (int)await connection.ExecuteScalarAsync(confirmationQuery, confirmationQueryParameters);
        //        if (userId > 0)
        //        {
        //            // Update user in database.
        //            var userQueryParameters = new
        //            {
        //                userId,
        //                PasswordManagerVersion = _passwordManagerVersion,
        //                salt,
        //                passwordHash
        //            };
        //            const string userQuery = @"
        //                update [Identity].Users
        //                set PasswordManagerVersion = @passwordManagerVersion, Salt = @salt, PasswordHash = @passwordHash
        //                where Id = @userId";
        //            await connection.ExecuteAsync(userQuery, userQueryParameters);
        //        }
        //        // TODO: Add invalid or expired code message.
        //    }
        //    await Task.FromResult(default(object));
        //    return new ResetPasswordResponse
        //    {
        //        PasswordValid = true,
        //        Messages = messages
        //    };
        //}


        //protected virtual async Task AddRolesAsync(IDbConnection Connection, User ApplicationUser)
        //{
        //    Logger.Log(CorrelationId, $"Adding roles to {ApplicationUser.Username} User object.");
        //    const string query = @"
        //        select r.Name
        //        from [Identity].UserRoles ur
        //        inner join [Identity].Users u on ur.UserId = u.Id
        //        inner join [Identity].Roles r on ur.RoleId = r.Id
        //        where u.Id = @id
        //        order by r.Name asc";
        //    var roles = await Connection.QueryAsync<Role>(query, ApplicationUser);
        //    foreach (var role in roles)
        //    {
        //        Logger.Log(CorrelationId, $"{role.Name} role");
        //        ApplicationUser.Roles.Add(role.Name);
        //    }
        //}


        //protected virtual async Task AddClaimsAsync(IDbConnection Connection, User ApplicationUser)
        //{
        //    Logger.Log(CorrelationId, $"Adding claims to {ApplicationUser.Username} User object.");
        //    const string query = @"
        //        select c.[Type], uc.[Value]
        //        from [Identity].UserClaims uc
        //        inner join [Identity].Users u on uc.UserId = u.Id
        //        inner join [Identity].Claims c on uc.ClaimId = c.Id
        //        where u.Id = @id
        //        order by c.[Type] asc, uc.[Value] asc";
        //    var claims = await Connection.QueryAsync<Entities.Claim>(query, ApplicationUser);
        //    foreach (var claim in claims)
        //    {
        //        Logger.Log(CorrelationId, $"Type = {claim.Type}, Value = {claim.Value} claim");
        //        if (!ApplicationUser.Claims.ContainsKey(claim.Type)) ApplicationUser.Claims.Add(claim.Type, new HashSet<string>(StringComparer.CurrentCultureIgnoreCase));
        //        ApplicationUser.Claims[claim.Type].Add(claim.Value);
        //    }
        //}


        //protected virtual void AddSecurityToken(User ApplicationUser)
        //{
        //    var claims = ApplicationUser.GetClaims();
        //    // Add valid time range claims.
        //    var credentialExpirationMinutes = ApplicationUser.Roles.Contains(Policy.Admin)
        //        ? AppSettings.AdminCredentialExpirationMinutes
        //        : AppSettings.NonAdminCredentialExpirationMinutes;
        //    var notBefore = DateTimeOffset.Now;
        //    var expires = DateTimeOffset.Now.AddMinutes(credentialExpirationMinutes);
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, notBefore.ToUnixTimeSeconds().ToString()));
        //    claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString()));
        //    // Create token header.
        //    // TODO: Should RngCryptoRfc2898 be used to get bytes from credential secret?
        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.CredentialSecret));
        //    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var tokenHeader = new JwtHeader(signingCredentials);
        //    // Create token payload.
        //    var tokenPayload = new JwtPayload(claims);
        //    var token = new JwtSecurityToken(tokenHeader, tokenPayload);
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    // Create signed token.
        //    ApplicationUser.SecurityToken = tokenHandler.WriteToken(token);
        //}
        // ReSharper restore ParameterHidesMember
    }
}
