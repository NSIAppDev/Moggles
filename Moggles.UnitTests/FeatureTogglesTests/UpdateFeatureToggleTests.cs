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
        public async Task ExistingFeatureToggleBasicDataIsUpdated()
        {
            //arrange
            var app = Application.Create("test", "DEV", false);
            app.AddFeatureToggle("TestToggle", "FirstNote", "WorkItemId", false);
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.Single();
            var updatedValue = new FeatureToggleUpdateModel
            {
                ApplicationId = app.Id,
                Id = toggle.Id,
                FeatureToggleName = "UpdatedFeatureToggleName",
                Notes = "Update",
                UserAccepted = true,
                Statuses = new List<FeatureToggleStatusUpdateModel>(),
                IsPermanent = true,
                WorkItemIdentifier = "UpdateWorkItemId"
            };

            //act
            await _featureToggleController.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.FeatureToggles.FirstOrDefault().ToggleName.Should().Be("UpdatedFeatureToggleName");
            savedApp.FeatureToggles.FirstOrDefault().Notes.Should().Be("Update");
            savedApp.FeatureToggles.FirstOrDefault().UserAccepted.Should().BeTrue();
            savedApp.FeatureToggles.FirstOrDefault().IsPermanent.Should().BeTrue();
            savedApp.FeatureToggles.FirstOrDefault().WorkItemIdentifier.Should().Be("UpdateWorkItemId");
        }

        [TestMethod]
        public async Task ChangingToggleName_ToExistingName_IsNotAllowed()
        {
            //arrange
            var app = Application.Create("test", "DEV", false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            app.AddFeatureToggle("t2", "", "workItemId1");
            await _appRepository.AddAsync(app);

            var toggle = app.FeatureToggles.FirstOrDefault(t => t.ToggleName == "t1");
            var updatedValue = new FeatureToggleUpdateModel { ApplicationId = app.Id, Id = toggle.Id, FeatureToggleName = "t2" };

            //act
            var result = await _featureToggleController.Update(updatedValue);

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            savedApp.FeatureToggles.FirstOrDefault(t => t.Id == toggle.Id).ToggleName.Should().Be("t1");
        }

        [TestMethod]
        public async Task FeatureToggleCanBeTurnedOn_ForAllExistingEnvironments()
        {
            //arrange
            var app = Application.Create("test", "DEV", false);
            app.AddDeployEnvironment("QA", false, false, false);
            app.AddFeatureToggle("t1", "", "workItemId1");
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


            //act
            await _featureToggleController.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            var statuses = savedApp.GetFeatureToggleStatuses(toggle.Id);
            statuses.Count.Should().Be(2);
            statuses.All(s => s.Enabled).Should().BeTrue();
        }

        [TestMethod]
        public async Task FeatureToggleUpdate_ByDifferentUser_UsernameChanged()
        {
            //arrange
            var app = Application.Create("test", "DEV", false);
            app.AddDeployEnvironment("QA", false, false, false);
            app.AddFeatureToggle("t1", "", "workItemId1");
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
                        Environment = "DEV",
                    },
                    new FeatureToggleStatusUpdateModel
                    {
                        Enabled = true,
                        Environment = "QA",
                    }
                }
            };

            //act
            await _featureToggleController.Update(updatedValue);

            //assert
            var savedApp = await _appRepository.FindByIdAsync(app.Id);
            var statuses = savedApp.GetFeatureToggleStatuses(toggle.Id);
            statuses.Count.Should().Be(2);
            statuses.All(s => s.UpdatedBy == "bla").Should().BeTrue();
        }
    }
}