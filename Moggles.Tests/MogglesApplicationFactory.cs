using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;


namespace Moggles.Tests
{
    public class MogglesApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder(new string[0])
                .UseApplicationInsights()
                .ConfigureKestrel(opt => opt.AddServerHeader = false)
                .UseStartup<TestStartup>();
        }
    }
}