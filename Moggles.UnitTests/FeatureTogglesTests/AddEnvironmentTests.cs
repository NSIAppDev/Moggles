using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moggles.UnitTests.Helpers;
using Moq;

namespace Moggles.UnitTests.FeatureTogglesTests
{
    [TestClass]
    public class AddEnvironmentTests
    {
        private IRepository<Application> _appRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
        }

        [TestMethod]
        public async Task ANewEnvironmentIsBeingCreatedWithProperInformation()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);
            var createdEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA", DefaultToggleValue = true, SortOrder = 99 };
            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

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
            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);
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

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

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

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            FeatureToggleHelper.GetFeatureToggleStatus(savedApp, "t1", "QA").Enabled.Should().BeFalse();
            FeatureToggleHelper.GetFeatureToggleStatus(savedApp, "t2", "QA").Enabled.Should().BeFalse();
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
            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            var result = await controller.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            FeatureToggleHelper.GetFeatureToggleStatus(savedApp, "t1", "QA").Enabled.Should().BeTrue();
            FeatureToggleHelper.GetFeatureToggleStatus(savedApp, "t2", "QA").Enabled.Should().BeTrue();
        }
    }
}