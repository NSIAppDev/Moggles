using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Data;
using Moggles.Domain;
using Moggles.Models;

namespace Moggles.UnitTests.ApplicationsTests
{
    [TestClass]
    public class ApplicationsTest
    {
        private static TogglesContext _context;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void BeforeTest()
        {
            _context = Fixture.GetTogglesContext(TestContext.TestName);
        }

        [TestCleanup]
        public void AfterTest()
        {
            //Different scopes sharing the same Incrementing Key

            //EnsureDeleted does not reset "identity" columns for InMemory database provider
            //Do not provide id's unless required,failing to do so will result in "failed to track .." error
            //see https://github.com/aspnet/EntityFrameworkCore/issues/4096 and https://github.com/aspnet/EntityFrameworkCore/issues/6872

            _context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void AddApplication_ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new ApplicationsController(_context);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = controller.AddApplication(new AddApplicationModel());
            
            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public void AddApplication_ApplicationIsBeingAdded()
        {
            //arrange
            var appModel = new AddApplicationModel {ApplicationName = "BCC"};

            var controller = new ApplicationsController(_context);

            //act
            controller.AddApplication(appModel);
            
            //assert
            _context.Applications.FirstOrDefault().AppName.Should().Be(appModel.ApplicationName);
        }

        [TestMethod]
        public void AddApplication_DefaultEnvironmentIsAdded()
        {
            //arrange
            var appModel = new AddApplicationModel { ApplicationName = "BCC", EnvironmentName = "Test", DefaultToggleValue = false };

            var controller = new ApplicationsController(_context);

            //act
            controller.AddApplication(appModel);

            //assert
            var appId = _context.Applications.FirstOrDefault(x => x.AppName.Equals(appModel.ApplicationName)).Id;

            var results = _context.DeployEnvironments.ToList();
            results.Count.Should().Be(1);
            results[0].EnvName.Should().Be("Test");
            results[0].DefaultToggleValue.Should().BeFalse();
        }


        [TestMethod]
        public void GetApplications_ReturnsAllExistingApplications()
        {
            //arrange
            var bccApp = new Application { AppName = "BCC" };
            var cmmApp = new Application { AppName = "CMM" };

            _context.Applications.Add(bccApp);
            _context.Applications.Add(cmmApp);
            _context.SaveChanges();

            var controller = new ApplicationsController(_context);

            //act
            var result = controller.GetAllApplications() as OkObjectResult;

            //assert
            result.Value.As<List<Application>>().Should().BeEquivalentTo(new List<Application>{bccApp, cmmApp});
        }

        [TestMethod]
        public void EditApp_AppIsBeingModified()
        {
            //arrange
            var app = new Application { Id = 1, AppName = "TestApp" };

            _context.Applications.Add(app);
            _context.SaveChanges();

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = app.Id,
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_context);

            //act
            var result = controller.UpdateApplication(updatedApp);

            //assert
            result.Should().BeOfType<OkResult>();
            app.AppName.Should().Be(updatedAppName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EditApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = new Application { Id = 1, AppName = "TestApp" };

            _context.Applications.Add(app);
            _context.SaveChanges();

            var updatedAppName = "TestAppUpdated";

            var updatedApp = new UpdateApplicationModel
            {
                Id = app.Id++,
                ApplicationName = updatedAppName
            };

            var controller = new ApplicationsController(_context);

            //act
            controller.UpdateApplication(updatedApp);

            //assert
            //throws InvalidOperationException
        }

        [TestMethod]
        public void DeleteApp_AppIsDeleted_EnvironmentsFeatureTogglesAndStatusesAreDeleted()
        {
            //arrange
            var app = new Application { Id = 1, AppName = "TestApp" };

            var environment = new DeployEnvironment { Application = app, ApplicationId = app.Id, EnvName = "TestEnv" };
            var environment2 = new DeployEnvironment { Application = app, ApplicationId = app.Id, EnvName = "TestEnv2" };
            var environment3 = new DeployEnvironment { Application = app, ApplicationId = app.Id, EnvName = "TestEnv3" };

            var featureStatus1 = new FeatureToggleStatus { Enabled = false, Id = 1, IsDeployed = false, Environment = environment, EnvironmentId = environment.Id };
            var featureStatus2 = new FeatureToggleStatus { Enabled = false, Id = 2, IsDeployed = false, Environment = environment2, EnvironmentId = environment2.Id };
            var featureStatus3 = new FeatureToggleStatus { Enabled = false, Id = 3, IsDeployed = false, Environment = environment3, EnvironmentId = environment3.Id };

            var feature = new FeatureToggle { Id = 1, Application = app, ApplicationId = app.Id, FeatureToggleStatuses = new List<FeatureToggleStatus> { featureStatus1, featureStatus2, featureStatus3 }, ToggleName = "Test" };

            _context.FeatureToggleStatuses.AddRange(featureStatus1, featureStatus2, featureStatus3);
            _context.Applications.Add(app);
            _context.DeployEnvironments.AddRange(environment, environment2, environment3);
            _context.FeatureToggles.Add(feature);
            _context.SaveChanges();

            var controller = new ApplicationsController(_context);

            //act
            var result = controller.RemoveApp(app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            _context.Applications.Count().Should().Be(0);
            _context.FeatureToggles.Count().Should().Be(0);
            _context.DeployEnvironments.Count().Should().Be(0);
            _context.FeatureToggleStatuses.Count().Should().Be(0);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteApp_WithInvalidID_ThrowsInvalidOperationException()
        {
            //arrange
            var app = new Application { Id = 1, AppName = "TestApp" };

            _context.Applications.Add(app);
            _context.SaveChanges();

            var controller = new ApplicationsController(_context);

            //act
            controller.RemoveApp(app.Id++);

            //assert
            //throws InvalidOperationException
        }
    }
}
