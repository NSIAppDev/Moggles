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
    public class AddFeatureToggleTests
    {
        private IRepository<Application> _appRepository;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryRepository();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            var controller = new FeatureTogglesController(_appRepository);
            controller.ModelState.AddModelError("error", "some error");

            //act
            var result = await controller.AddFeatureToggle(new AddFeatureToggleModel());

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenFeatureAlreadyExists()
        {
            //arrange
            var app = Application.Create("bcc", "dev", false);
            app.AddFeatureToggle("TestToggle", string.Empty);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenApplicationNotSpecified()
        {
            //arrange
            var newFeatureToggle = new AddFeatureToggleModel { FeatureToggleName = "TestToggle" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task FeatureToggleIsCreated()
        {
            //arrange
            var app = Application.Create("tst", "dev", false);
            await _appRepository.AddAsync(app);
            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            var toggle = (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault();
            toggle.Should().NotBeNull();
            toggle.ToggleName.Should().Be("TestToggle");
        }

        [TestMethod]
        public async Task FeatureToggleStatus_IsCreated_ForEveryEnvironment()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false);
            app.AddDeployEnvironment("QA", false);

            var newFeatureToggle = new AddFeatureToggleModel { ApplicationId = app.Id, FeatureToggleName = "TestToggle" };

            await _appRepository.AddAsync(app);

            var controller = new FeatureTogglesController(_appRepository);

            //act
            var result = await controller.AddFeatureToggle(newFeatureToggle);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.FirstOrDefault().FeatureToggleStatuses.Count.Should().Be(2);
        }
    }
}