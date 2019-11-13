using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace Moggles.Tests
{
    [TestClass]
    public class GenericIntegrationTests
    {
        private MogglesApplicationFactory<TestStartup> _factory;
        private HttpClient _client;

        [TestInitialize]
        public void BeforeEach()
        {
            _factory = new MogglesApplicationFactory<TestStartup>();
            var factory = _factory.WithWebHostBuilder(b =>
            {
                b.UseSolutionRelativeContentRoot(Environment.CurrentDirectory);
                b.ConfigureTestServices(services =>
                {
                    services.AddControllersWithViews().AddApplicationPart(typeof(Startup).Assembly);
                });
            });
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task SiteRootIsAccessible()
        {
            var response = await _client.GetAsync("/");
            //var content = response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
            Utils.ClearStorage();
        }
    }
}
