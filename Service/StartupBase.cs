using System;
using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Data;
using ErikTheCoder.Identity.Domain;
using ErikTheCoder.Logging;
using ErikTheCoder.Utilities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using EnvironmentName = ErikTheCoder.Utilities.EnvironmentName;
using IEmailSettings = ErikTheCoder.Identity.Domain.IEmailSettings;


namespace ErikTheCoder.Identity.Service
{
    public abstract class StartupBase : AspNetCore.Middleware.StartupBase
    {
        // Define configuration values that do not vary per environment, and therefore are not saved in appSettings.json.
        [UsedImplicitly] private const int _clockSkewMinutes = 5;


        protected abstract string ConfirmationUrl { get; }


        [UsedImplicitly]
        public virtual void ConfigureServices(IServiceCollection Services)
        {
            // JwtBearer cannot be referenced in .NET Standard class library (it requires .NET Core App Framework).
            // Implement this method in service that hosts the Identity Account Controller.

            // <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
            // <PackageReference Include="System.IdentityModel.Tokens.Jwt" />
            // using System.Text;
            // using Microsoft.AspNetCore.Authentication.JwtBearer;
            // using Microsoft.AspNetCore.Authorization;
            // using Microsoft.AspNetCore.Mvc.Authorization;
            // using Microsoft.IdentityModel.Tokens;

            //var appSettings = ParseConfigurationFile<IAppSettings, AppSettings>();
            //// Require custom or JWT authentication token.
            //// The JWT token specifies the security algorithm used when it was signed (by Identity service).
            //Services.AddAuthentication(AuthenticationHandler.AuthenticationScheme).AddErikTheCoderAuthentication(Options =>
            //{
            //    Options.Identities = appSettings.AuthenticationIdentities;
            //    Options.ForwardDefaultSelector = HttpContext =>
            //    {
            //        // Forward to JWT authentication if custom token is not present.
            //        var token = string.Empty;
            //        if (HttpContext.Request.Headers.TryGetValue(AuthenticationHandler.HttpHeaderName, out var authorizationValues)) token = authorizationValues.ToString();
            //        return token.StartsWith(AuthenticationHandler.TokenPrefix)
            //            ? AuthenticationHandler.AuthenticationScheme
            //            : JwtBearerDefaults.AuthenticationScheme;
            //    };
            //})
            //.AddJwtBearer(Options =>
            //{
            //    Options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.CredentialSecret)),
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        ValidateLifetime = true,
            //        ClockSkew = TimeSpan.FromMinutes(_clockSkewMinutes)
            //    };
            //});
            //// Add MVC, filters, policies, and configure routing.
            //Services.AddMvc(Options => Options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()))); // Require authorization (permission to access controller actions).
            //Services.AddAuthorizationCore(Options => Options.UseErikTheCoderPolicies()); // Authorize using policies that examine claims.
            //Services.AddRouting(Options => Options.LowercaseUrls = true);

            //// Don't use memory cache in services.  Use it in website instead to avoid two network I/O hops:
            ////   Website -> Service -> Database
            ////   Website <- Service <- Database
            //// This guarantees service always provide current data.

            //// Configure dependency injection.
            //ConfigureDependencyInjection(Services, appSettings);
        }


        [UsedImplicitly]
        public virtual void Configure(IApplicationBuilder ApplicationBuilder, IHostingEnvironment HostingEnvironment, IAppSettings AppSettings, ILogger Logger)
        {
            var correlationId = Guid.NewGuid();
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


        [UsedImplicitly]
        protected virtual void ConfigureDependencyInjection(IServiceCollection Services, IAppSettings AppSettings)
        {
            Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Services.AddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();
            Services.AddSingleton(ServiceProvider => ParseConfigurationFile<IAppSettings, AppSettings>());
            Services.AddSingleton<ILogger>(ServiceProvider => new ConcurrentDatabaseLogger(ServiceProvider.GetService<IAppSettings>().Logger, new SqlDatabase()));
            Services.AddSingleton<ILoggedDatabase>(ServiceProvider => new LoggedSqlDatabase(ServiceProvider.GetService<ILogger>(), ServiceProvider.GetService<IAppSettings>().Database));
            Services.AddSingleton<IThreadsafeRandom, ThreadsafeCryptoRandom>();
            Services.AddSingleton<IEmailSettings>(ServiceProvider => new EmailSettings
            {
                EnableSsl = AppSettings.Email.EnableSsl,
                Host = AppSettings.Email.Host,
                Port = AppSettings.Email.Port,
                Username = AppSettings.Email.Username,
                ConfirmationUrl = ConfirmationUrl,
                Password = AppSettings.Email.Password,
                From = AppSettings.Email.From
            });
            Services.AddSingleton<IIdentityFactory, IdentityFactory>();
            Services.AddScoped(ServiceProvider => ServiceProvider.GetService<IdentityFactory>().CreateIdentityRepository());
        }
    }
}
