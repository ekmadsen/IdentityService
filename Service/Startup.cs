﻿using System;
using System.Text;
using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Identity.Service.PasswordManagers;
using ErikTheCoder.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using EnvironmentName = ErikTheCoder.ServiceContract.EnvironmentName;


namespace ErikTheCoder.Identity.Service
{
    public class Startup
    {
        // Define configuration values that do not vary per environment, and therefore are not saved in appsettings.json.
        private const int _clockSkewMinutes = 5;


        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection Services)
        {
            Guid correlationId = Guid.NewGuid();
            // Require authentication token.
            // The token specifies the security algorithm used when it was signed.
            Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Program.AppSettings.CredentialSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(_clockSkewMinutes)
                };
            });
            // Require authorization (permission to access controller actions) using custom claims.
            Services.AddAuthorization(Options => Options.UseErikTheCoderPolicies());
            // Add MVC and configure routing.
            Services.AddMvc().AddJsonOptions(Options => Options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()); // Preserve case of property names.
            Services.AddRouting(Options => Options.LowercaseUrls = true);
            // Don't use memory cache in services.  Use it in website instead to avoid two network I/O hops:
            //   Website -> Service -> Database
            //   Website <- Service <- Database
            // This guarantees service always provide current data.
            // Create logger and password managers.
            ILogger logger = new ConcurrentDatabaseLogger(Program.AppSettings.Logger);
            logger.Log(correlationId, $"{Program.AppSettings.Logger.ProcessName} starting.");
            IPasswordManagerVersions passwordManagerVersions = new PasswordManagerVersions();
            // Configure dependency injection.
            Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Services.AddSingleton(typeof(IAppSettings), Program.AppSettings);
            Services.AddSingleton(typeof(ILogger), logger);
            Services.AddSingleton(typeof(IPasswordManagerVersions), passwordManagerVersions);
        }


        [UsedImplicitly]
        public void Configure(IApplicationBuilder ApplicationBuilder, IHostingEnvironment HostingEnvironment)
        {
            // Require authentication (identification of user).
            ApplicationBuilder.UseAuthentication();
			// Log parameters, metrics, and performance of each request.
            ApplicationBuilder.UseErikTheCoderLogging(Options => Options.LogRequestParameters = Program.AppSettings.Logger.TraceLogLevel == LogLevel.Debug);
            // Configure exception handling.
            ApplicationBuilder.UseErikTheCoderExceptionHandling(Options =>
            {
                Options.AppName = Program.AppSettings.Logger.AppName;
                Options.ProcessName = Program.AppSettings.Logger.ProcessName;
                Options.ExceptionResponseFormat = ExceptionResponseFormat.Json;
                Options.IncludeDetails = !HostingEnvironment.IsEnvironment(EnvironmentName.Prod);
            });
            // Use MVC.
            ApplicationBuilder.UseMvc();
        }
    }
}