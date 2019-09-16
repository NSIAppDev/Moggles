using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Domain;
using Moggles.Models;
using MogglesContracts;

namespace Moggles.Tests
{
    [TestClass]
    public class CacheRefreshIntegrationTests
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
        public async Task Publishes_Message_With_CacheRefresh_Command()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "test", EnvironmentName = "testEnv", DefaultToggleValue = false };
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);
            response.EnsureSuccessStatusCode();

            var app = await response.Content.ReadAsJsonAsync<Application>();

            var refreshCacheModel = new RefreshCacheModel
            {
                EnvName = "DEV",
                ApplicationId = app.Id
            };

            //act
            var response2 = Utils.PostAsJsonAsync(_client, "/api/CacheRefresh", refreshCacheModel).Result;
            response2.EnsureSuccessStatusCode();

            //TODO: check to see how to test sending of messages on the BUS
            //TODO: use mass transit test harness package
        }
    }

    public class TestCacheRefreshSubscriber : IConsumer<RefreshTogglesCache>
    {
        public bool MessageRecieved { get; set; }
        public RefreshTogglesCache TheMessage { get; set; }

        public Task Consume(ConsumeContext<RefreshTogglesCache> context)
        {
            TheMessage = context.Message;
            MessageRecieved = true;
            return Task.CompletedTask;
        }
    }
}
