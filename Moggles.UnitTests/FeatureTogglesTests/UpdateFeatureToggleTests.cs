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
    public class UpdateFeatureToggleTests
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
        public async Task ExistingFeatureToggleBasicDataIsUpdated()
        {
            //arrange
            var app = Application.Create("test", "DEV", false, "username");
            app.AddFeatureToggle("TestToggle", "FirstNote", "username");
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.Single();
            var updatedValue = new FeatureToggleUpdateModel
            {
                ApplicationId = app.Id, Id = toggle.Id, FeatureToggleName = "UpdatedFeatureToggleName", Notes = "Update", UserAccepted = true,
                Statuses = new List<FeatureToggleStatusUpdateModel>(), IsPermanent = true
            };

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            await controller.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.FeatureToggles.FirstOrDefault().ToggleName.Should().Be("UpdatedFeatureToggleName");
            savedApp.FeatureToggles.FirstOrDefault().Notes.Should().Be("Update");
            savedApp.FeatureToggles.FirstOrDefault().UserAccepted.Should().BeTrue();
            savedApp.FeatureToggles.FirstOrDefault().IsPermanent.Should().BeTrue();
        }

        [TestMethod]
        public async Task ChangingToggleName_ToExistingName_IsNotAllowed()
        {
            //arrange
            var app = Application.Create("test", "DEV", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            app.AddFeatureToggle("t2", "", "username");
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.FirstOrDefault(t => t.ToggleName == "t1");
            var updatedValue = new FeatureToggleUpdateModel { ApplicationId = app.Id, Id = toggle.Id, FeatureToggleName = "t2" };

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            var result = await controller.Update(updatedValue);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.FeatureToggles.FirstOrDefault(t => t.Id == toggle.Id).ToggleName.Should().Be("t1");
        }

        [TestMethod]
        public async Task FeatureToggleCanBeTurnedOn_ForAllExistingEnvironments()
        {
            //arrange
            var app = Application.Create("test", "DEV", false, "username");
            app.AddDeployEnvironment("QA", false, "username");
            app.AddFeatureToggle("t1", "", "username");
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.Single();
            var updatedValue = new FeatureToggleUpdateModel
            {
                ApplicationId = app.Id,
                Id = toggle.Id,
                FeatureToggleName = "t1",
                Statuses = new List<FeatureToggleStatusUpdateModel>
                {
                    new FeatureToggleStatusUpdateModel
                    {
                        Enabled = true,
                        Environment = "DEV"
                    },
                    new FeatureToggleStatusUpdateModel
                    {
                        Enabled = true,
                        Environment = "QA"
                    }
                }
            };

            var controller = new FeatureTogglesController(_appRepository, _httpContextAccessor);

            //act
            await controller.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            var statuses = savedApp.GetFeatureToggleStatuses(toggle.Id);
            statuses.Count.Should().Be(2);
            statuses.All(s => s.Enabled).Should().BeTrue();
        }
    }
}