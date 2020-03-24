using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moq;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class DeleteEnvironmentTests
    {
        private IRepository<Application> _appRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private IRepository<ToggleSchedule> _toggleScheduleRepository;
        private FeatureTogglesController _featureToggleController;
        private ToggleSchedulerController _toggleSchedulerConstroller;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;


        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
            _toggleSchedulerConstroller = new ToggleSchedulerController(_toggleScheduleRepository, _appRepository, _httpContextAccessor);
            _featureToggleController = new FeatureTogglesController(_appRepository, _httpContextAccessor, _toggleScheduleRepository);
        }

        [TestMethod]
        public async Task EnvironmentIsDeleted_FeatureToggleStatusForThatEnvironmentIsDeletedForAllToggles()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            app.AddFeatureToggle("t2", "", "workItemId2");
            app.AddFeatureToggle("t3", "", "workItemId3");
            await _appRepository.AddAsync(app);


            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };

            //act
            var result = await _featureToggleController.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count.Should().Be(0);
            savedApp.FeatureToggles.Count(ft => ft.FeatureToggleStatuses.Count > 0).Should().Be(0);
        }

        [TestMethod]
        public async Task EnvironmentIsDeleted_FeatureTogglesAreNotDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            app.AddFeatureToggle("t2", "", "workItemId2");
            app.AddFeatureToggle("t3", "", "workItemId3");
            await _appRepository.AddAsync(app);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };
            //act
            var result = await _featureToggleController.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count.Should().Be(0);
            savedApp.FeatureToggles.Count.Should().Be(3);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Environment does not exist!")]
        public async Task WhenEnvironmentIsDeletedWithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "BLA"
            };

            //act
            await _featureToggleController.RemoveEnvironment(environmentToRemove);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task WhenEnvironmentIsDeleted_RemoveAllSchedulersForEnvironment()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false, false, false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            var toggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);
            await _toggleSchedulerConstroller.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1" },
                Environments = new List<string> { "DEV", "QA" },
                ScheduleDate = date,
                State = true
            });

            //act
            var result = await _featureToggleController.RemoveEnvironment(new DeleteEnvironmentModel { ApplicationId = app.Id, EnvName = "DEV" });

            //assert
            result.Should().BeOfType<OkResult>();
            var schedulers = (await _toggleScheduleRepository.GetAllAsync()).Where(ft => ft.Environments.Contains("DEV"));
            schedulers.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task DeletingAnEnvironment_DeletsEnvironmentNameForAllReasons()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            app.AddDeployEnvironment("Env", false, false, true);
            await _appRepository.AddAsync(app);
            var featureToggle = app.FeatureToggles.First();
            app.UpdateFeatureToggleReasonsToChange(featureToggle.Id, "user", "reason1", new List<string> { "TestEnv", "Env" });

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };
            //act
            var result = await _featureToggleController.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count.Should().Be(1);
            savedApp.FeatureToggles.Count.Should().Be(1);
            var ft = savedApp.FeatureToggles.First();
            ft.ReasonsToChange.Single().Environments.Count.Should().Be(1);
            ft.ReasonsToChange.Single().Environments.First().Should().Be("Env");

        }

        [TestMethod]
        public async Task DeletingAnEnvironment_WhenReasonHasOnlyOneEnvironment_DeletesReason()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            app.AddDeployEnvironment("Env", false, false, true);
            await _appRepository.AddAsync(app);
            var featureToggle = app.FeatureToggles.First();
            app.UpdateFeatureToggleReasonsToChange(featureToggle.Id, "user", "reason1", new List<string> { "TestEnv"});

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };
            //act
            var result = await _featureToggleController.RemoveEnvironment(environmentToRemove);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count.Should().Be(1);
            savedApp.FeatureToggles.Count.Should().Be(1);
            var ft = savedApp.FeatureToggles.First();
            ft.ReasonsToChange.Count.Should().Be(0);
        }
    }
}