using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.Tests
{
    [TestClass]
    public class ApplicationIntegrationTests
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
        public void WillRejectRequestWithEmptyAppName()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "" };

            //act
            var response = Utils.PostAsJsonAsync(_client, "/api/applications/add", appModel).Result;

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void WillRejectRequestWithAppNameBiggerThan100Chars()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = string.Join("", Enumerable.Repeat("x", 101)) };

            //act
            var response = Utils.PostAsJsonAsync(_client, "/api/applications/add", appModel).Result;
      
            //assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void CreateAppSuccessfully()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "test", EnvironmentName = "testEnv", DefaultToggleValue = false};

            //act
            var response = Utils.PostAsJsonAsync(_client, "/api/applications/add", appModel).Result;
            response.EnsureSuccessStatusCode();

            var readResponse = _client.GetAsync("/api/applications").Result;
            var apps = readResponse.Content.ReadAsJsonAsync<List<Application>>().Result;

            //assert
            var app = apps.FirstOrDefault(a => a.AppName == "test");
            app.Should().NotBeNull();
            app.Id.Should().NotBe(Guid.Empty);
        }

        [TestMethod]
        public void CreatingANewAppWillAlsoCreateADefaultEnvironment()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "test", EnvironmentName = "testEnv", DefaultToggleValue = false };

            //act
            var response = Utils.PostAsJsonAsync(_client, "/api/applications/add", appModel).Result;
            response.EnsureSuccessStatusCode();
            var app = response.Content.ReadAsJsonAsync<Application>().Result;

            var readResponse = _client.GetAsync($"/api/FeatureToggles/environments?applicationId={app.Id}").Result;
            readResponse.EnsureSuccessStatusCode();
            var envs = readResponse.Content.ReadAsJsonAsync<List<string>>().Result;

            //assert
            envs.Should().BeEquivalentTo("testEnv");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
        }
    }
}
