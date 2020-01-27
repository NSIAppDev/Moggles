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
        private IRepository<ToggleSchedule> _toggleScheduleRepository;
        private FeatureTogglesController _featureToggleController;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
            _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
            _featureToggleController = new FeatureTogglesController(_appRepository, _httpContextAccessor, _toggleScheduleRepository);
        }

        [TestMethod]
        public async Task ANewEnvironmentIsBeingCreatedWithProperInformation()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            await _appRepository.AddAsync(app);
            var createdEnvironment = new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "QA", DefaultToggleValue = true, SortOrder = 99 };

            //act
            var result = await _featureToggleController.AddEnvironment(createdEnvironment);

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
            _featureToggleController.ModelState.AddModelError("error", "some error");

            //act
            var result = await _featureToggleController.AddEnvironment(new AddEnvironmentModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenEnvironmentAlreadyExists()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);


            //act
            var result = await _featureToggleController.AddEnvironment(new AddEnvironmentModel { ApplicationId = app.Id, EnvName = "DEV" });

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

            //act
            var result = await _featureToggleController.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            FeatureToggleTestHelper.GetFeatureToggleStatus(savedApp, "t1", "QA").Enabled.Should().BeFalse();
            FeatureToggleTestHelper.GetFeatureToggleStatus(savedApp, "t2", "QA").Enabled.Should().BeFalse();
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

            //act
            var result = await _featureToggleController.AddEnvironment(newEnvironment);

            //assert
            result.Should().BeOfType<OkResult>();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            FeatureToggleTestHelper.GetFeatureToggleStatus(savedApp, "t1", "QA").Enabled.Should().BeTrue();
            FeatureToggleTestHelper.GetFeatureToggleStatus(savedApp, "t2", "QA").Enabled.Should().BeTrue();
        }
    }
}