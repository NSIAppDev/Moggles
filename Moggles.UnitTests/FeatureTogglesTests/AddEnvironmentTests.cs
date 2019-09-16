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
    public class AddEnvironmentTests
    {
        private IRepository<Application> _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
        }

        [TestMethod]
        public async Task ANewEnvironmentIsBeingCreatedWithProperInformation()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);
            var createdEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA", DefaultToggleValue = true, SortOrder = 99 };
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(createdEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.DeploymentEnvironments.Count.Should().Be(2);
            var qaEnv = savedApp.DeploymentEnvironments.FirstOrDefault(x => x.EnvName.Equals("QA"));
            qaEnv.Should().NotBeNull();
            qaEnv.DefaultToggleValue.Should().BeTrue();
            qaEnv.SortOrder.Should().Be(99);
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new FeatureTogglesController(_appRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddEnvironment(new AddEnvironmentModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenEnvironmentAlreadyExists()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "DEV" });

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task EveryExistingFeatureToggle_IsMarkedAs_Off_ForTheNewEnvironment()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddFeatureToggle("t1", string.Empty);
            app.AddFeatureToggle("t2", string.Empty);
            await _appRepository.AddAsync(app);

            var newEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.GetFeatureToggleStatus("t1", "QA").Enabled.Should().BeFalse();
            savedApp.GetFeatureToggleStatus("t2", "QA").Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task EveryExistingFeatureToggle_IsMarkedAs_On_ForTheNewEnvironment_WhenTheDefaultValueForTheToggleIsTrue()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            var newEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA", DefaultToggleValue = true };
            app.AddFeatureToggle("t1", string.Empty);
            app.AddFeatureToggle("t2", string.Empty);

            await _appRepository.AddAsync(app);
            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.GetFeatureToggleStatus("t1", "QA").Enabled.Should().BeTrue();
            savedApp.GetFeatureToggleStatus("t2", "QA").Enabled.Should().BeTrue();
        }
    }
}