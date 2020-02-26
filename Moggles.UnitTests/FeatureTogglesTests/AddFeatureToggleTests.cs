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
    public class AddFeatureToggleTests
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
        public async Task ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            _featureToggleController.ModelState.AddModelError("error", "some error");

            //act
            var result = await _featureToggleController.AddFeatureToggle(new AddFeatureToggleModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenFeatureAlreadyExists()
        {
            //arrange
            var app = Application.Create("bcc", "dev", false);
            app.AddFeatureToggle("TestToggle", string.Empty, "workItemId1");

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);


            //act
            var result = await _featureToggleController.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenApplicationNotSpecified()
        {
            //arrange
            var newFeatureToggle = new AddFeatureToggleModel { FeatureToggleName = "TestToggle" };


            //act
            var result = await _featureToggleController.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task FeatureToggleIsCreated()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);
            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle", WorkItemIdentifier = "1234" };


            //act
            var result = await _featureToggleController.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            var toggle = (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault();
            toggle.Should().NotBeNull();
            toggle.ToggleName.Should().Be("TestToggle");
            toggle.WorkItemIdentifier.Should().Be("1234");
        }

        [TestMethod]
        public async Task FeatureToggleStatus_IsCreated_ForEveryEnvironment()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false, false, false);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault().FeatureToggleStatuses.Count.Should().Be(2);
        }

        [TestMethod]
        public async Task FeatureToggleStatus_IsCreated_WithDefaultUsername()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false, false, false);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault().FeatureToggleStatuses.Count.Should().Be(2);
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault().FeatureToggleStatuses.First().UpdatedbyUser.Should().Be("System");

        }
    }
}