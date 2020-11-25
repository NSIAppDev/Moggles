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
                NewEnvName = updatedEnvironmentName,
                RequireReasonForChangeWhenToggleEnabled = false,
                RequireReasonForChangeWhenToggleDisabled = true
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.First().EnvName.Should().Be(updatedEnvironmentName);
        }

        [TestMethod]
        public async Task WhenNewInvironmentName_MatchesAnExistingEnvrionmentWithDifferentCase_EnvironmentNameIsChanged()
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
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).DeploymentEnvironments.First().EnvName.Should().Be("dev");
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
            app.AddFeatureToggle("t1", string.Empty, "workItemId1");
            app.AddFeatureToggle("t2", string.Empty, "workItemId2");
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

        [TestMethod]
        public async Task WhenEnvironmentNameIsChanged_EnvironmentNameForToggleSchedulersIsChanged()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", true);
            await _appRepository.AddAsync(app);
            app.AddFeatureToggle("t1", string.Empty, "workItemId1");
            app.AddFeatureToggle("t2", string.Empty, "workItemId2");
            var t1 = app.FeatureToggles.ToList().FirstOrDefault(ft => ft.ToggleName == "t1");
            app.SetToggle(t1.Id, "DEV", false, "bla");
            var schedule = ToggleSchedule.Create("TestApp", "t1", new[] { "DEV" }, true, new DateTime(2018, 1, 1, 15, 30, 0), "updatedBy", true);
            await _toggleScheduleRepository.AddAsync(schedule);

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
            var ts1 = (await _toggleScheduleRepository.GetAllAsync()).FirstOrDefault(fts => fts.ToggleName == "t1");
            ts1.Environments.Should().Contain("QA");
            ts1.Environments.Should().NotContain("DEV");
        }

        [TestMethod]
        public async Task DefaultToggleValueChanged_NextToggleHasDefaultValue()
        {
            var app = Application.Create("TestApp", "DEV", true);
            await _appRepository.AddAsync(app);
            app.AddFeatureToggle("t1", string.Empty, "workItemId1");
            var t1 = app.FeatureToggles.ToList().FirstOrDefault(ft => ft.ToggleName == "t1");

            var updatedEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = app.Id,
                DefaultToggleValue = false,
                InitialEnvName = "DEV",
                NewEnvName="DEV"
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updatedEnvironment);

            //assert
            app.AddFeatureToggle("t2", string.Empty, "workItemId1");
            var t2 = app.FeatureToggles.ToList().FirstOrDefault(ft => ft.ToggleName == "t2");
            t1.GetFeatureToggleStatusForEnv("DEV").Enabled.Should().Be(true);
            t2.GetFeatureToggleStatusForEnv("DEV").Enabled.Should().Be(false);
        }

        [TestMethod]
        public async Task EnvironmentSortOrderChanged_WhenPositionChangedToLeft_OrderIsChanged()
        {

            //arrange
            var application = Application.Create("Test", "DEV", false);
            await _appRepository.AddAsync(application);
            application.DeploymentEnvironments.First().SortOrder = 0;
            application.AddDeployEnvironment("QA", false, false, false, sortOrder:1);
            application.AddDeployEnvironment("LIVE", false, false, false, sortOrder:2);

            await _appRepository.UpdateAsync(application);

            var updateEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = application.Id,
                DefaultToggleValue = false,
                InitialEnvName = "LIVE",
                NewEnvName = "LIVE",
                MoveToLeft = true,
                MoveToRight = false
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updateEnvironment);
            application.DeploymentEnvironments = application.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();

            //assert
            var environment = application.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == "LIVE");
            environment.SortOrder.Should().Be(1);
            var leftEnvironmentIndex = application.DeploymentEnvironments.IndexOf(environment) - 1;
            var leftEnvironment = application.DeploymentEnvironments[leftEnvironmentIndex];
            leftEnvironment.EnvName.Should().Be("DEV");
            leftEnvironment.SortOrder.Should().Be(0);
            var rightEnvironmentIndex = application.DeploymentEnvironments.IndexOf(environment) + 1;
            var rightEnvironment = application.DeploymentEnvironments[rightEnvironmentIndex];
            rightEnvironment.EnvName.Should().Be("QA");
            rightEnvironment.SortOrder.Should().Be(2);
        }

        [TestMethod]
        public async Task EnvironmentSortOrderChanged_WhenPositionChangedToRight_OrderIsChanged()
        {
            //arrange
            var application = Application.Create("Test", "DEV", false);
            await _appRepository.AddAsync(application);
            application.DeploymentEnvironments.First().SortOrder = 0;
            application.AddDeployEnvironment("QA", false, false, false, sortOrder: 1);
            application.AddDeployEnvironment("LIVE", false, false, false, sortOrder: 2);

            await _appRepository.UpdateAsync(application);

            var updateEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = application.Id,
                DefaultToggleValue = false,
                InitialEnvName = "DEV",
                NewEnvName = "DEV",
                MoveToLeft = false,
                MoveToRight = true
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updateEnvironment);
            application.DeploymentEnvironments = application.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();

            //assert
            var environment = application.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == "DEV");
            environment.SortOrder.Should().Be(1);
            var leftEnvironmentIndex = application.DeploymentEnvironments.IndexOf(environment) - 1;
            var leftEnvironment = application.DeploymentEnvironments[leftEnvironmentIndex];
            leftEnvironment.EnvName.Should().Be("QA");
            leftEnvironment.SortOrder.Should().Be(0);
            var rightEnvironmentIndex = application.DeploymentEnvironments.IndexOf(environment) + 1;
            var rightEnvironment = application.DeploymentEnvironments[rightEnvironmentIndex];
            rightEnvironment.EnvName.Should().Be("LIVE");
            rightEnvironment.SortOrder.Should().Be(2);
        }

        [TestMethod]
        public async Task WhenMovetoRightAndLeftAreTrue_EnvironmentSortOrderDoesNotChange()
        {

            //arrange
            var application = Application.Create("Test", "DEV", false);
            await _appRepository.AddAsync(application);
            application.DeploymentEnvironments.First().SortOrder = 0;
            application.AddDeployEnvironment("QA", false, false, false, sortOrder: 1);
            application.AddDeployEnvironment("LIVE", false, false, false, sortOrder: 2);

            await _appRepository.UpdateAsync(application);

            var updateEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = application.Id,
                DefaultToggleValue = false,
                InitialEnvName = "DEV",
                NewEnvName = "DEV",
                MoveToLeft = true,
                MoveToRight = true
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updateEnvironment);

            //assert
            var environments = application.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();
            environments.Should().BeEquivalentTo(application.DeploymentEnvironments);
        }

        [TestMethod]
        public async Task EnvironmentIsOnTheFirstPosition_WhenMoveToLeftIsTrue_SortOrderForEnvironmentIsNotChanged()
        {
            //arrange
            var application = Application.Create("Test", "DEV", false);
            await _appRepository.AddAsync(application);
            application.DeploymentEnvironments.First().SortOrder = 0;
            application.AddDeployEnvironment("QA", false, false, false, sortOrder: 1);
            application.AddDeployEnvironment("LIVE", false, false, false, sortOrder: 2);

            await _appRepository.UpdateAsync(application);

            var updateEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = application.Id,
                DefaultToggleValue = false,
                InitialEnvName = "DEV",
                NewEnvName = "DEV",
                MoveToLeft = true,
                MoveToRight = false
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updateEnvironment);

            //assert
            var environments = application.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();
            environments.Should().BeEquivalentTo(application.DeploymentEnvironments);
            var environment = application.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == "DEV");
            environment.SortOrder.Should().Be(0);
        }

        [TestMethod]
        public async Task EnvironmentIsOnTheLastPosition_WhenMoveToRightIsTrue_SortOrderForEnvironmentIsNotChanged()
        {
            //arrange
            var application = Application.Create("Test", "DEV", false);
            await _appRepository.AddAsync(application);
            application.DeploymentEnvironments.First().SortOrder = 0;
            application.AddDeployEnvironment("QA", false, false, false, sortOrder: 1);
            application.AddDeployEnvironment("LIVE", false, false, false, sortOrder: 2);

            await _appRepository.UpdateAsync(application);

            var updateEnvironment = new UpdateEnvironmentModel
            {
                ApplicationId = application.Id,
                DefaultToggleValue = false,
                InitialEnvName = "LIVE",
                NewEnvName = "LIVE",
                MoveToLeft = false,
                MoveToRight = true
            };

            //act
            var result = await _featureToggleController.UpdateEnvironment(updateEnvironment);

            //assert
            var environments = application.DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();
            environments.Should().BeEquivalentTo(application.DeploymentEnvironments);
            var environment = application.DeploymentEnvironments.FirstOrDefault(e => e.EnvName == "LIVE");
            environment.SortOrder.Should().Be(2);
        }

    }
}