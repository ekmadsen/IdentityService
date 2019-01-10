﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Identity.Contract;
using ErikTheCoder.Identity.Contract.Requests;
using ErikTheCoder.Identity.Contract.Responses;
using ErikTheCoder.Identity.Service.PasswordManagers;
using ErikTheCoder.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ControllerBase = ErikTheCoder.AspNetCore.Middleware.ControllerBase;
using User = ErikTheCoder.Identity.Contract.User;


namespace ErikTheCoder.Identity.Service.Controllers
{
    [Authorize(Policy = Policy.Admin)]
    [Route("account")]
    public class AccountController : ControllerBase, IAccountService
    {
        private const string _invalidCredentials = "Invalid username or password.";
        private const int _passwordManagerVersion = 2;
        private readonly IPasswordManagerVersions _passwordManagerVersions;


        public AccountController(IAppSettings AppSettings, ILogger Logger, IPasswordManagerVersions PasswordManagerVersions) :
            base(AppSettings, Logger)
        {
            _passwordManagerVersions = PasswordManagerVersions;
        }


        // Access ASP.NETCore's Request property via this.Request.
        // ReSharper disable ParameterHidesMember
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<User> LoginAsync([FromBody] LoginRequest Request)
        {
            // Validate password against hash stored in database.
            var queryParameters = new { Request.Username };
            // TODO: Also require account to be confirmed.
            const string query = @"
                select u.id, u.Username, u.PasswordManagerVersion, u.Salt, u.PasswordHash, u.EmailAddress, u.FirstName, u.LastName
                from [Identity].Users u
                where u.Username = @username
                and u.Enabled = 1";
            User user;
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                user = await connection.QuerySingleOrDefaultAsync<User>(query, queryParameters);
                if (user != null)
                {
                    IPasswordManager passwordManager = _passwordManagerVersions[user.PasswordManagerVersion];
                    if (passwordManager.Validate(Request.Password, user.Salt, user.PasswordHash))
                    {
                        // Password valid.
                        Logger.Log(CorrelationId, $"{Request.Username} user authenticated.");
                        AddRoles(connection, user);
                        AddSecurityToken(user);
                    }
                    else
                    {
                        // Password invalid.
                        Logger.Log(CorrelationId, $"{Request.Username} user not authenticated.  {_invalidCredentials}");
                        user = null;
                    }
                }
            }
            return user;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<RegisterResponse> RegisterAsync([FromBody] RegisterRequest Request)
        {
            // TODO: Validate email address and username are available.
            // TODO: Run multiple SQL insert statements in a transaction.  Rollback if an exception occurs.
            // Validate password meets complexity requirements.
            IPasswordManager passwordManager = _passwordManagerVersions[_passwordManagerVersion];
            (bool valid, List<string> messages) = passwordManager.ValidateComplexity(Request.Password);
            if (!valid)
            {
                return new RegisterResponse
                {
                    PasswordValid = false,
                    Messages = messages
                };
            }
            // Add user to database.
            (string salt, string passwordHash) = passwordManager.Hash(Request.Password);
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
            int userId;
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                userId = (int)await connection.ExecuteScalarAsync(addUserQuery, addUserQueryParameters);
            }
            // Add confirmation to database.
            string code = Guid.NewGuid().ToString();
            var confirmationQueryParameters = new
            {
                userId,
                Request.EmailAddress,
                code,
                Sent = DateTime.Now
            };
            const string confirmationQuery = @"
                insert into [Identity].UserConfirmations (UserId, EmailAddress, Code, Sent)
                values (@userId, @emailAddress, @code, @sent)";
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(confirmationQuery, confirmationQueryParameters);
            }
            // Send confirmation email.
            using (MailMessage email = new MailMessage())
            {
                // TODO: Upgrade confirmation message from plain text to HTML.
                // TODO: Move confirmation message text to database.
                email.From = new MailAddress(AppSettings.Email.From);
                email.To.Add(new MailAddress(Request.EmailAddress));
                email.Subject = "account activation";
                // TODO: Use MVC routing to create email confirmation hyperlink.
                string confirmationUrl = string.Format(AppSettings.Email.ConfirmationUrl, Request.EmailAddress, code);
                email.Body = $"Click {confirmationUrl} to confirm your email address.";
                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = AppSettings.Email.Host,
                    Port = AppSettings.Email.Port,
                    EnableSsl = AppSettings.Email.EnableSsl,
                    Credentials = new NetworkCredential(AppSettings.Email.Username, AppSettings.Email.Password)
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


        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task ConfirmAsync([FromBody] ConfirmRequest Request)
        {
            // TODO: Run multiple SQL update statements in a transaction.  Rollback if an exception occurs.
            // Update confirmation in database.
            var confirmationQueryParameters = new
            {
                Request.EmailAddress,
                Request.Code,
                Received = DateTime.Now
            };
            // TODO: Add expiration for confirmation code.
            const string confirmationQuery = @"
                declare @userId int
                update [Identity].UserConfirmations
                set Received = @received, @userId = UserId
                where EmailAddress = @emailAddress
                and Code = @code
                select @userId";
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                int userId = (int)await connection.ExecuteScalarAsync(confirmationQuery, confirmationQueryParameters);
                if (userId > 0)
                {
                    // Update user in database.
                    var userQueryParameters = new { userId };
                    const string userQuery = @"
                        update [Identity].Users
                        set Confirmed = 1
                        where Id = @userId";
                    await connection.ExecuteAsync(userQuery, userQueryParameters);
                }
                // TODO: Add invalid or expired code message.
            }
        }


        [AllowAnonymous]
        [HttpPost("forgotpassword")]
        public async Task ForgotPasswordAsync([FromBody] ForgotPasswordRequest Request)
        {
            // TODO: Validate email address but don't disclose to user whether an account exists for the email address.
            // TODO: Include username in email message.
            // Add confirmation to database.
            string code = Guid.NewGuid().ToString();
            var confirmationQueryParameters = new
            {
                Request.EmailAddress,
                code,
                Sent = DateTime.Now
            };
            const string confirmationQuery = @"
                declare @userId int
                select @userId = u.Id
                from [Identity].Users u
                where u.EmailAddress = @emailAddress
                insert into [Identity].UserConfirmations (UserId, EmailAddress, Code, Sent)
                values (@userId, @emailAddress, @code, @sent)";
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                await connection.ExecuteAsync(confirmationQuery, confirmationQueryParameters);
            }
            // Send confirmation email.
            using (MailMessage email = new MailMessage())
            {
                // TODO: Upgrade confirmation message from plain text to HTML.
                // TODO: Move confirmation message text to database.
                email.From = new MailAddress(AppSettings.Email.From);
                email.To.Add(new MailAddress(Request.EmailAddress));
                email.Subject = "password reset";
                // TODO: Use MVC routing to create email confirmation hyperlink.
                string confirmationUrl = string.Format(AppSettings.Email.ConfirmationUrl, Request.EmailAddress, code);
                email.Body = $"Click {confirmationUrl} to reset your password.";
                using (SmtpClient smtpClient = new SmtpClient
                {
                    Host = AppSettings.Email.Host,
                    Port = AppSettings.Email.Port,
                    EnableSsl = AppSettings.Email.EnableSsl,
                    Credentials = new NetworkCredential(AppSettings.Email.Username, AppSettings.Email.Password)
                })
                {
                    await smtpClient.SendMailAsync(email);
                }
            }
        }


        [AllowAnonymous]
        [HttpPost("resetpassword")]
        public async Task<ResetPasswordResponse> ResetPasswordAsync([FromBody] ResetPasswordRequest Request)
        {
            // TODO: Run multiple SQL update statements in a transaction.  Rollback if an exception occurs.
            // Validate password meets complexity requirements.
            IPasswordManager passwordManager = _passwordManagerVersions[_passwordManagerVersion];
            (bool valid, List<string> messages) = passwordManager.ValidateComplexity(Request.NewPassword);
            if (!valid)
            {
                return new ResetPasswordResponse
                {
                    PasswordValid = false,
                    Messages = messages
                };
            }
            (string salt, string passwordHash) = passwordManager.Hash(Request.NewPassword);
            // Update confirmation in database.
            var confirmationQueryParameters = new
            {
                Request.EmailAddress,
                Request.Code,
                Received = DateTime.Now
            };
            // TODO: Add expiration for reset code.
            const string confirmationQuery = @"
                declare @userId int
                update [Identity].UserConfirmations
                set Received = @received, @userId = UserId
                where EmailAddress = @emailAddress
                and Code = @code
                select @userId";
            using (SqlConnection connection = new SqlConnection(AppSettings.Database))
            {
                await connection.OpenAsync();
                int userId = (int)await connection.ExecuteScalarAsync(confirmationQuery, confirmationQueryParameters);
                if (userId > 0)
                {
                    // Update user in database.
                    var userQueryParameters = new
                    {
                        userId,
                        PasswordManagerVersion = _passwordManagerVersion,
                        salt,
                        passwordHash
                    };
                    const string userQuery = @"
                        update [Identity].Users
                        set PasswordManagerVersion = @passwordManagerVersion, Salt = @salt, PasswordHash = @passwordHash
                        where Id = @userId";
                    await connection.ExecuteAsync(userQuery, userQueryParameters);
                }
                // TODO: Add invalid or expired code message.
            }
            await Task.FromResult(default(object));
            return new ResetPasswordResponse
            {
                PasswordValid = true,
                Messages = messages
            };
        }


        private void AddRoles(SqlConnection Connection, User ApplicationUser)
        {
            Logger.Log(CorrelationId, $"Adding roles to {ApplicationUser.Username} User object.");
            // TODO: Retrieve user roles from database.
            ApplicationUser.Roles.Add("Admin");
            ApplicationUser.Roles.Add("Web Master");
            ApplicationUser.Roles.Add("Debugger");
            foreach (string role in ApplicationUser.Roles) { Logger.Log(CorrelationId, $"{role} role"); }
        }


        private void AddSecurityToken(User ApplicationUser)
        {
            List<Claim> claims = ApplicationUser.GetClaims();
            // Add valid time range claims.
            int credentialExpirationMinutes = ApplicationUser.Roles.Contains(Policy.Admin)
                ? AppSettings.AdminCredentialExpirationMinutes
                : AppSettings.NonAdminCredentialExpirationMinutes;
            DateTimeOffset notBefore = DateTimeOffset.Now;
            DateTimeOffset expires = DateTimeOffset.Now.AddMinutes(credentialExpirationMinutes);
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, notBefore.ToUnixTimeSeconds().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expires.ToUnixTimeSeconds().ToString()));
            // Create token header.
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.CredentialSecret));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtHeader tokenHeader = new JwtHeader(signingCredentials);
            // Create token payload.
            JwtPayload tokenPayload = new JwtPayload(claims);
            JwtSecurityToken token = new JwtSecurityToken(tokenHeader, tokenPayload);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            // Create signed token.
            ApplicationUser.SecurityToken = tokenHandler.WriteToken(token);
        }
        // ReSharper restore ParameterHidesMember
    }
}
