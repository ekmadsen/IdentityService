using ErikTheCoder.AspNetCore.Middleware;
using ErikTheCoder.AspNetCore.Middleware.Settings;
using ErikTheCoder.Data;
using ErikTheCoder.Identity.Domain;
using ErikTheCoder.Logging;
using ErikTheCoder.Utilities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IEmailSettings = ErikTheCoder.Identity.Domain.IEmailSettings;


namespace ErikTheCoder.Identity.Service
{
    public abstract class StartupBase : AspNetCore.Middleware.StartupBase
    {
        protected abstract string ConfirmationUrl { get; }


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
