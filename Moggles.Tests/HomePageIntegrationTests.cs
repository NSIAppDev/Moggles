using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moggles.Tests
{
    [TestClass]
    public class GenericIntegrationTests
    {
        private TestFixture<TestStartup> _fixture;
        private HttpClient _client;

        [TestInitialize]
        public void BeforeEach()
        {
            _fixture = new TestFixture<TestStartup>(relativeTargetProjectParentDir: "Moggles", reflectBaseType: true);
            _client = _fixture.Client;
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
            _fixture.Dispose();
        }
    }
}
