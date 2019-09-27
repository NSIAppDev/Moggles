using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Utils.ClearStorage();
            _factory = new MogglesApplicationFactory<TestStartup>();
            var factory = _factory.WithWebHostBuilder(b =>
            {
                b.UseSolutionRelativeContentRoot(Environment.CurrentDirectory);
                b.ConfigureTestServices(services => { services.AddMvc().AddApplicationPart(typeof(Startup).Assembly); });
            });
            _client = factory.CreateClient();
        }

        [Ignore]
        [TestMethod]
        public async Task SiteRootIsAccessible()
        {
            var response = await _client.GetAsync("/");
            response.EnsureSuccessStatusCode();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
        }
    }
}
