using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
            _factory = new MogglesApplicationFactory<TestStartup>();
            var factory = _factory.WithWebHostBuilder(b =>
            {
                b.UseSolutionRelativeContentRoot(Environment.CurrentDirectory);
                b.ConfigureTestServices(services =>
                {
                    services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
                });
            });
            _client = factory.CreateClient();
        }

        [TestMethod]
        public async Task WillRejectRequestWithEmptyAppName()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "" };

            //act
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task WillRejectRequestWithAppNameBiggerThan100Chars()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = string.Join("", Enumerable.Repeat("x", 101)) };

            //act
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);

            //assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task CreateAppSuccessfully()
        {

            var appModel = new AddApplicationModel { ApplicationName = "tst", EnvironmentName = "testEnv", DefaultToggleValue = false };

            //act
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);
            response.EnsureSuccessStatusCode();

            var readResponse = _client.GetAsync("/api/applications").Result;
            var apps = await readResponse.Content.ReadAsJsonAsync<List<Application>>();

            //assert
            var app = apps.FirstOrDefault(a => a.AppName == "tst");
            app.Should().NotBeNull();
            app.Id.Should().NotBe(Guid.Empty);
        }

        [TestMethod]
        public async Task CreatingANewAppWillAlsoCreateADefaultEnvironment()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "tst", EnvironmentName = "testEnv", DefaultToggleValue = false };

            //act
            var response = await _client.PostAsJsonAsync("/api/applications/add", appModel);
            response.EnsureSuccessStatusCode();
            var app = await response.Content.ReadAsJsonAsync<Application>();

            var readResponse = _client.GetAsync($"/api/FeatureToggles/environments?applicationId={app.Id}").Result;
            readResponse.EnsureSuccessStatusCode();
            var envs = await readResponse.Content.ReadAsJsonAsync<List<string>>();

            //assert
            envs.Should().BeEquivalentTo("testEnv");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _factory.Dispose();
            Utils.ClearStorage();
        }
    }
}
