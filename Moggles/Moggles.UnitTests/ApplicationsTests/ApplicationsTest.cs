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
    }
}
