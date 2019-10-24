using FluentAssertions;
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

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
        }

        [TestMethod]
        public async Task GetToggles_ReturnsAList_WithAllTheToggles_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("BCC", "dev", false, "username");
            app.AddFeatureToggle("TestToggle", "TestNotes", "username", true);
            app.AddFeatureToggle("TestToggle2", "TestNotes2", "username");

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.GetToggles(app.Id) as OkObjectResult;
            var list = result.Value as IEnumerable<FeatureToggleViewModel>;

            //assert
            list.Count().Should().Be(2);
            var toggle = list.FirstOrDefault(t => t.ToggleName == "TestToggle");
            toggle.Notes.Should().Be("TestNotes");
            toggle.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, 200);
            toggle.UserAccepted.Should().Be(false);
            toggle.IsPermanent.Should().Be(true);
        }

        [TestMethod]
        public async Task GetToggles_ReturnsAList_WithAllTheToggles_AndTheStatusesOfThoseToggles_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false, "username");
            app.AddDeployEnvironment("QA", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            var toggle = app.FeatureToggles.Single();
            app.SetToggle(toggle.Id, "DEV", true, "username");
            app.SetToggle(toggle.Id, "QA", true, "username");
            //TODO: mark the toggle as deployed on these environments
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.GetToggles(app.Id) as OkObjectResult;
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
            var app = Application.Create("TestApp", "DEV", false, "username");

            var expectedEnvNames = new List<string>
            {
                "DEV",
                "QA",
                "SBX",
                "TRN",
                "LIVE"
            };

            app.AddDeployEnvironment("QA", false, "username");
            app.AddDeployEnvironment("SBX", false, "username");
            app.AddDeployEnvironment("TRN", false, "username");
            app.AddDeployEnvironment("LIVE", false, "username");
            await _appRepository.AddAsync(app);

            //act
            var controller = new FeatureTogglesController(_appRepository);
            var results = await controller.GetEnvironments(app.Id) as OkObjectResult;

            //assert
            results.Value.Should().BeEquivalentTo(expectedEnvNames);
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsAList_WithAllTheDistinctEnvironments_ForTheGivenApplication()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false, "username");

            app.AddDeployEnvironment("QA", false, "username");
            app.AddDeployEnvironment("TRN", false, "username");

            var expectedEnvNames = new List<string> { "DEV", "QA", "TRN" };

            await _appRepository.AddAsync(app);
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var results = await controller.GetEnvironments(app.Id) as OkObjectResult;

            //assert
            results.Value.Should().BeEquivalentTo(expectedEnvNames);
        }

        [TestMethod]
        public async Task GetApplicationFeatureToggles_ReturnsFeatureToggleState_ForTheGivenApplicationNameAndEnvironmentName()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false, "username");
            app.AddDeployEnvironment("QA", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            var toggle = app.FeatureToggles.FirstOrDefault(f => f.ToggleName == "t1");
            app.SetToggle(toggle.Id, "DEV", true, "username");
            app.SetToggle(toggle.Id, "QA", false, "username");

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.GetApplicationFeatureToggles(app.AppName, "DEV") as OkObjectResult;
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
            var app = Application.Create("TestApp", "DEV", false, "username");
            app.AddDeployEnvironment("QA", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            var toggle = app.FeatureToggles.FirstOrDefault(f => f.ToggleName == "t1");
            app.SetToggle(toggle.Id, "DEV", false, "username");
            app.SetToggle(toggle.Id, "QA", true, "username");
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.GetApplicationFeatureToggleValue(app.AppName, "QA", "t1") as OkObjectResult;
            var okObjectResult = result.Value as ApplicationFeatureToggleViewModel;

            //assert
            okObjectResult.FeatureToggleName.Should().Be("t1");
            okObjectResult.IsEnabled.Should().BeTrue();
        }
    }
}
