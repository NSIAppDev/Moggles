using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Domain;
using Moggles.Models;
using MogglesContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Moggles.Tests
{
    [TestClass]
    public class CacheRefreshIntegrationTests : IPublishObserver
    {
        private MogglesApplicationFactory<TestStartup> _factory;
        private HttpClient _client;

        private readonly List<object> _messages = new List<object>();

        [TestInitialize]
        public void BeforeEach()
        {
            _factory = new MogglesApplicationFactory<TestStartup>();
            var factory = _factory.WithWebHostBuilder(b =>
            {
                b.UseSolutionRelativeContentRoot(Environment.CurrentDirectory);
                b.ConfigureTestServices(services => { services.AddControllersWithViews().AddApplicationPart(typeof(Startup).Assembly); });
            });
            _client = factory.CreateClient();
            var bus = (IBusControl)factory.Server.Services.GetRequiredService(typeof(IBusControl));
            bus.ConnectPublishObserver(this);
        }

        [TestCleanup]
        public void AfterEach()
        {
            Utils.ClearStorage();
            _messages.Clear();
        }

        [TestMethod]
        public async Task Publishes_Message_With_CacheRefresh_Command()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "tst", EnvironmentName = "testEnv", DefaultToggleValue = false };
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);
            response.EnsureSuccessStatusCode();

            var app = await response.Content.ReadAsJsonAsync<Application>();

            var refreshCacheModel = new RefreshCacheModel
            {
                EnvName = "DEV",
                ApplicationId = app.Id
            };

            //act
            var response2 = await _client.PostAsJsonAsync("/api/CacheRefresh", refreshCacheModel);
            response2.EnsureSuccessStatusCode();

            var msg = (RefreshTogglesCache)_messages.FirstOrDefault(m => m is RefreshTogglesCache);
            msg.Should().NotBeNull();
            msg.Environment.Should().Be("DEV");
            msg.ApplicationName.Should().Be("tst");
        }

        public Task PrePublish<T>(PublishContext<T> context)
            where T : class
        {
            _messages.Add(context.Message);
            return Task.CompletedTask;
            // called right before the message is published (sent to exchange or topic)
        }

        public Task PostPublish<T>(PublishContext<T> context)
            where T : class
        {
            return Task.CompletedTask;
            // called after the message is published (and acked by the broker if RabbitMQ)
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception)
            where T : class
        {
            return Task.CompletedTask;
            // called if there was an exception publishing the message
        }
    }
}
