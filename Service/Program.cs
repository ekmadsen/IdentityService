using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;


namespace ErikTheCoder.Identity.Service
{
    [UsedImplicitly]
    public static class Program
    {
        public static void Main(string[] Args)
        {
            // Build and run web host.
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder(Args);
            webHostBuilder.UseKestrel();
            webHostBuilder.UseStartup<Startup>();
            IWebHost webHost = webHostBuilder.Build();
            webHost.Run();
        }
    }
}
