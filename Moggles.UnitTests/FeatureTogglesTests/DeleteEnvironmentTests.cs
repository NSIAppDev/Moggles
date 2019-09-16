﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class DeleteEnvironmentTests
    {
        private IRepository<Application> _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
        }

        [TestMethod]
        public async Task EnvironmentIsDeleted_FeatureToggleStatusForThatEnvironmentIsDeletedForAllToggles()
        {
            //arrange
            var app = Application.Create("TestApp", "TestEnv", false);
            app.AddFeatureToggle("t1", "");
            app.AddFeatureToggle("t2", "");
            app.AddFeatureToggle("t3", "");
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };

            //act
            var result = await controller.RemoveEnvironment(environmentToRemove);

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
            app.AddFeatureToggle("t1", "");
            app.AddFeatureToggle("t2", "");
            app.AddFeatureToggle("t3", "");
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "TestEnv"
            };
            //act
            var result = await controller.RemoveEnvironment(environmentToRemove);

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

            var controller = new FeatureTogglesController(_appRepository);

            var environmentToRemove = new DeleteEnvironmentModel
            {
                ApplicationId = app.Id,
                EnvName = "BLA"
            };

            //act
            await controller.RemoveEnvironment(environmentToRemove);

            //assert
            //throws InvalidOperationException
        }
    }
}