using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Models;
using System.Linq;
using System.Threading.Tasks;
using Moggles.Domain;

namespace Moggles.UnitTests.ApplicationsTests
{
    [TestClass]
    public class ApplicationsTest
    {
        private InMemoryRepository _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryRepository();
        }

        [TestMethod]
        public async Task AddApplication_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new ApplicationsController(_appRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddApplication(new AddApplicationModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task AddApplication_ApplicationIsBeingAdded()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "BCC" };

            var controller = new ApplicationsController(_appRepository);

            //act
            await controller.AddApplication(appModel);

            //assert
            _appRepository.Applications.FirstOrDefault()?.AppName.Should().Be(appModel.ApplicationName);
        }

        [TestMethod]
        public async Task AddApplication_DefaultEnvironmentIsAdded()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "BCC", EnvironmentName = "Test", DefaultToggleValue = false };

            var controller = new ApplicationsController(_appRepository);

            //act
            await controller.AddApplication(appModel);

            //assert
            var app = _appRepository.Applications.FirstOrDefault(x => x.AppName.Equals(appModel.ApplicationName));

            Debug.Assert(app != null, nameof(app) + " != null");
            var results = app.DeploymentEnvironments.ToList();
            results.Count.Should().Be(1);
            results[0].EnvName.Should().Be("Test");
            results[0].DefaultToggleValue.Should().BeFalse();
        }


        [TestMethod]
        public async Task GetApplications_ReturnsAllExistingApplications()
        {
            //arrange
            var bccApp = new Application { AppName = "BCC" };
            var cmmApp = new Application { AppName = "CMM" };

            await _appRepository.AddAsync(bccApp);
            await _appRepository.AddAsync(cmmApp);

            var controller = new ApplicationsController(_appRepository);

            //act
            var result = await controller.GetAllApplications() as OkObjectResult;

            //assert
            Debug.Assert(result != null, nameof(result) + " != null");
            result.Value.As<List<Application>>().Should().BeEquivalentTo(new List<Application> { bccApp, cmmApp });
        }

        [TestMethod]
        public async Task EditApp_AppIsBeingModified()
        {
            //arrange
            var app = new Application { Id = Guid.NewGuid(), AppName = "TestApp" };

            await _appRepository.AddAsync(app);

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = app.Id,
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_appRepository);

            //act
            var result = await controller.UpdateApplication(updatedApp);

            //assert
            result.Should().BeOfType<OkResult>();
            app.AppName.Should().Be(updatedAppName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task EditApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = new Application { Id = Guid.NewGuid(), AppName = "TestApp" };

            await _appRepository.AddAsync(app);

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = Guid.NewGuid(),
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_appRepository);

            //act
            await controller.UpdateApplication(updatedApp);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public async Task DeleteApp_AppIsDeleted()
        {
            //arrange
            var app = new Application { Id = Guid.NewGuid(), AppName = "TestApp" };

            var environment = new DeployEnvironment { Id = Guid.NewGuid(), EnvName = "TestEnv" };
            var environment2 = new DeployEnvironment { Id = Guid.NewGuid(), EnvName = "TestEnv2" };
            var environment3 = new DeployEnvironment { Id = Guid.NewGuid(), EnvName = "TestEnv3" };

            app.DeploymentEnvironments.AddRange(new[] { environment, environment2, environment3 });

            var featureStatus1 = new FeatureToggleStatus { Enabled = false, Id = Guid.NewGuid(), IsDeployed = false, EnvironmentId = environment.Id };
            var featureStatus2 = new FeatureToggleStatus { Enabled = false, Id = Guid.NewGuid(), IsDeployed = false, EnvironmentId = environment2.Id };
            var featureStatus3 = new FeatureToggleStatus { Enabled = false, Id = Guid.NewGuid(), IsDeployed = false, EnvironmentId = environment3.Id };

            var feature = new FeatureToggle
            {
                Id = Guid.NewGuid(),
                FeatureToggleStatuses = new List<FeatureToggleStatus> {featureStatus1, featureStatus2, featureStatus3},
                ToggleName = "Test"
            };

            app.FeatureToggles.Add(feature);

            await _appRepository.AddAsync(app);

            var controller = new ApplicationsController(_appRepository);

            //act
            var result = await controller.RemoveApp(app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            _appRepository.Applications.Count().Should().Be(0);
          
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeleteApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = new Application { Id = Guid.NewGuid(), AppName = "TestApp" };

            await _appRepository.AddAsync(app);

            var controller = new ApplicationsController(_appRepository);

            //act
            await controller.RemoveApp(Guid.NewGuid());

            //assert
            //throws InvalidOperationException
        }
    }
}
