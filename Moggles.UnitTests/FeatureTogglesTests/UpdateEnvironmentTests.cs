using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Data.NoDb;
using Moggles.Domain;
using Moggles.Models;
using Moggles.UnitTests.Helpers;
using NoDb;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class UpdateEnvironmentTests
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
        public async Task EnvironmentIsBeingModified()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var updatedEnvironmentName = "QA";

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "DEV",
                NewEnvName = updatedEnvironmentName
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.First().EnvName.Should().Be(updatedEnvironmentName);
        }

        [TestMethod]
        public async Task WhenNewInvironmentName_MatchesAnExistingEnvrionment_TheChangeIsRejected()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "DEV",
                NewEnvName = "dev"
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.First().EnvName.Should().Be("DEV");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        public async Task WhenEnvironmentIsModifiedWithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var updatedEnvironmentName = "QA";

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "BLA",
                NewEnvName = updatedEnvironmentName
            };

            //act
            await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task WhenEnvironmentIsModified_EnvironmentNameForFeatureToggleStatusesUpdated()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", true);
            await _appRepository.AddAsync(app);
            app.AddFeatureToggle("t1", string.Empty);
            app.AddFeatureToggle("t2", string.Empty);
            var t1 = app.FeatureToggles.ToList().FirstOrDefault(ft => ft.ToggleName == "t1");
            app.SetToggle(t1.Id, "DEV", false, "bla");

            var updatedEnvironmentName = "QA";

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                InitialEnvName = "DEV",
                NewEnvName = updatedEnvironmentName
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            var t1Statuses = t1.FeatureToggleStatuses.First();
            t1Statuses.EnvironmentName.Should().Be("QA");
            t1Statuses.Enabled.Should().BeFalse();
            var t2 = savedApp.FeatureToggles.ToList().FirstOrDefault(ft => ft.ToggleName == "t2");
            var t2Statuses = t2.FeatureToggleStatuses.First();
            t2Statuses.EnvironmentName.Should().Be("QA");
            t2Statuses.Enabled.Should().BeTrue();
        }
    }
}