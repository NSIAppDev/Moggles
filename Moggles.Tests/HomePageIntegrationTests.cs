using System.Net.Http;
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
            _client = _factory.CreateClient();
        }

        [TestMethod]
        public void SiteRootIsAccessible()
        {
            var response = _client.GetAsync("/").Result;
            response.EnsureSuccessStatusCode();

        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
        }
    }
}
