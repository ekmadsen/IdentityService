﻿using System;
using System.IO;
using System.Text;
using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Identity.Service.PasswordManagers;
using ErikTheCoder.Logging;
using ErikTheCoder.ServiceContract;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
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
            IAppSettings appSettings = ParseConfigurationFile();
            // Require custom or JWT authentication token.
            // The JWT token specifies the security algorithm used when it was signed (by Identity service).
            Services.AddAuthentication(AuthenticationHandler.AuthenticationScheme).AddErikTheCoderAuthentication(Options =>
            {
                Options.Identities = appSettings.AuthenticationIdentities;
                Options.ForwardDefaultSelector = HttpContext =>
                {
                    // Forward to JWT authentication if custom token is not present.
                    string token = string.Empty;
                    if (HttpContext.Request.Headers.TryGetValue(AuthenticationHandler.HttpHeaderName, out StringValues authorizationValues)) token = authorizationValues.ToString();
                    return token.StartsWith(AuthenticationHandler.TokenPrefix)
                        ? AuthenticationHandler.AuthenticationScheme
                        : JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddJwtBearer(Options =>
            {
                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.CredentialSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(_clockSkewMinutes)
                };
            });
            // Add MVC, filters, policies, and configure routing.
            Services.AddMvc(Options => Options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()))); // Require authorization (permission to access controller actions).
            Services.AddAuthorization(Options => Options.UseErikTheCoderPolicies()); // Authorize using policies that examine claims.
            Services.AddRouting(Options => Options.LowercaseUrls = true);
            // Don't use memory cache in services.  Use it in website instead to avoid two network I/O hops:
            //   Website -> Service -> Database
            //   Website <- Service <- Database
            // This guarantees service always provide current data.
            // Configure dependency injection (DI).  Have DI framework create singleton instances so they're properly disposed.
            Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Services.AddSingleton(ServiceProvider => ParseConfigurationFile());
            Services.AddSingleton<ILogger>(ServiceProvider => new ConcurrentDatabaseLogger(ServiceProvider.GetService<IAppSettings>().Logger));
            Services.AddSingleton<ISafeRandom, SafeRandom>();
            Services.AddSingleton<IPasswordManagerVersions, PasswordManagerVersions>();
            Services.AddSingleton<IDatabase>(ServiceProvider => new SqlDatabase(ServiceProvider.GetService<IAppSettings>().Database));
        }


        [UsedImplicitly]
        public void Configure(IApplicationBuilder ApplicationBuilder, IHostingEnvironment HostingEnvironment, IAppSettings AppSettings, ILogger Logger)
        {
            Guid correlationId = Guid.NewGuid();
            Logger.Log(correlationId, $"{AppSettings.Logger.ProcessName} starting.");
            // Require authentication (identification of user).
            ApplicationBuilder.UseAuthentication();
            // Log parameters, authentication, metrics, and performance of each request.
            // ReSharper disable once ImplicitlyCapturedClosure
            ApplicationBuilder.UseErikTheCoderLogging(Options => Options.LogRequestParameters = AppSettings.Logger.TraceLogLevel == LogLevel.Debug);
            // Configure exception handling.
            ApplicationBuilder.UseErikTheCoderExceptionHandling(Options =>
            {
                Options.AppName = AppSettings.Logger.AppName;
                Options.ProcessName = AppSettings.Logger.ProcessName;
                Options.ExceptionResponseFormat = ExceptionResponseFormat.Json;
                Options.IncludeDetails = !HostingEnvironment.IsEnvironment(EnvironmentName.Prod);
            });
            // Use MVC.
            ApplicationBuilder.UseMvc();
        }


        private static IAppSettings ParseConfigurationFile()
        {
            const string environmentalVariableName = "ASPNETCORE_ENVIRONMENT";
            string environment = Environment.GetEnvironmentVariable(environmentalVariableName) ?? Microsoft.AspNetCore.Hosting.EnvironmentName.Development;
            string directory = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? string.Empty;
            string configurationFile = Path.Combine(directory, "appSettings.json");
            if (!File.Exists(configurationFile)) throw new Exception($"Configuration file not found at {configurationFile}.");
            JObject configuration = JObject.Parse(File.ReadAllText(configurationFile));
            return configuration.GetValue(environment).ToObject<AppSettings>();
        }
    }
}
