using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class FeatureToggleQueriesTests
    {
        private IRepository<Application> _appRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private IRepository<ToggleSchedule> _toggleScheduleRepository;
        private FeatureTogglesController _featureToggleController;
        

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
            _featureToggleController = new FeatureTogglesController(_appRepository, _httpContextAccessor, _toggleScheduleRepository);
        }

        [TestMethod]
        public async Task GetToggles_ReturnsAList_WithAllTheToggles_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("BCC", "dev", false);
            app.AddFeatureToggle("TestToggle", "TestNotes", true, "workItemID");
            app.AddFeatureToggle("TestToggle2", "TestNotes2");

            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.GetToggles(app.Id) as OkObjectResult;
            var list = result.Value as IEnumerable<FeatureToggleViewModel>;

            //assert
            list.Count().Should().Be(2);
            var toggle = list.FirstOrDefault(t => t.ToggleName == "TestToggle");
            toggle.Notes.Should().Be("TestNotes");
            toggle.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, 200);
            toggle.UserAccepted.Should().Be(false);
            toggle.IsPermanent.Should().Be(true);
            toggle.WorkItemIdentifier.Should().Be("workItemID");

        }

        [TestMethod]
        public async Task GetToggles_ReturnsAList_WithAllTheToggles_AndTheStatusesOfThoseToggles_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", "");
            var toggle = app.FeatureToggles.Single();
            app.SetToggle(toggle.Id, "DEV", true, "username");
            app.SetToggle(toggle.Id, "QA", true, "username");
            //TODO: mark the toggle as deployed on these environments
            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.GetToggles(app.Id) as OkObjectResult;
            result.Should().NotBeNull();
            var list = result.Value as IEnumerable<FeatureToggleViewModel>;

            //assert
            list.First().Statuses.Count.Should().Be(2);
            var devStatus = list.First().Statuses.FirstOrDefault(s => s.Environment == "DEV");
            devStatus.Enabled.Should().BeTrue();
            //TODO: enable this
            //devStatus.IsDeployed.Should().BeTrue();

            var qaStatus = list.First().Statuses.FirstOrDefault(s => s.Environment == "QA");
            qaStatus.Enabled.Should().BeTrue();
            //TODO: enable this
            //qaStatus.IsDeployed.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsAList_WithAllTheEnvironments_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);

            var expectedEnvNames = new List<string>
            {
                "DEV",
                "QA",
                "SBX",
                "TRN",
                "LIVE"
            };

            app.AddDeployEnvironment("QA", false);
            app.AddDeployEnvironment("SBX", false);
            app.AddDeployEnvironment("TRN", false);
            app.AddDeployEnvironment("LIVE", false);
            await _appRepository.AddAsync(app);

            //act
            var results = await _featureToggleController.GetEnvironments(app.Id) as OkObjectResult;

            //assert
            var returnedEnvs = (results.Value as IEnumerable<DeployEnvironment>).Select(env => env.EnvName);
            returnedEnvs.Should().BeEquivalentTo(expectedEnvNames);
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsAList_WithAllTheDistinctEnvironments_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);

            app.AddDeployEnvironment("QA", true);
            app.AddDeployEnvironment("TRN", false);

            var expectedEnvNames = new List<string> { "DEV", "QA", "TRN" };
            var expectedDefaultValues = new List<bool> { false, true, false };
            
            await _appRepository.AddAsync(app);

            //act
            var results = await _featureToggleController.GetEnvironments(app.Id) as OkObjectResult;


            //assert
            var okObjectresult = results.Value as IEnumerable<DeployEnvironment>;
            okObjectresult.ToList().Count().Should().Be(3);
            var returnedEnvs = okObjectresult.Select(env => env.EnvName);
            returnedEnvs.Should().BeEquivalentTo(expectedEnvNames);
            var returnedDefaultValues = okObjectresult.Select(env => env.DefaultToggleValue);
            returnedDefaultValues.Should().BeEquivalentTo(expectedDefaultValues);
        }

        [TestMethod]
        public async Task GetApplicationFeatureToggles_ReturnsFeatureToggleState_ForTheGivenApplicationNameAndEnvironmentName()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", "");
            var toggle = app.FeatureToggles.FirstOrDefault(f => f.ToggleName == "t1");
            app.SetToggle(toggle.Id, "DEV", true, "username");
            app.SetToggle(toggle.Id, "QA", false, "username");

            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.GetApplicationFeatureToggles(app.AppName, "DEV") as OkObjectResult;
            var okObjectResult = result.Value as IEnumerable<ApplicationFeatureToggleViewModel>;

            //assert
            okObjectResult.ToList().Count.Should().Be(1);
            okObjectResult.First().FeatureToggleName.Should().Be("t1");
            okObjectResult.First().IsEnabled.Should().BeTrue();
        }

        [TestMethod]
        public async Task GetApplicationFeatureToggleValue_ReturnsTheStatus_OfTheGivenFeatureToggle()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", "");
            var toggle = app.FeatureToggles.FirstOrDefault(f => f.ToggleName == "t1");
            app.SetToggle(toggle.Id, "DEV", false, "username");
            app.SetToggle(toggle.Id, "QA", true, "username");
            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.GetApplicationFeatureToggleValue(app.AppName, "QA", "t1") as OkObjectResult;
            var okObjectResult = result.Value as ApplicationFeatureToggleViewModel;

            //assert
            okObjectResult.FeatureToggleName.Should().Be("t1");
            okObjectResult.IsEnabled.Should().BeTrue();
        }
    }
}
