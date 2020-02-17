using System;
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
    public class RemoveFeatureToggleTests
    {
        private IRepository<Application> _appRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private IRepository<ToggleSchedule> _toggleScheduleRepository;
        private FeatureTogglesController _featureToggleController;
        private ToggleSchedulerController _toggleSchedulerConstroller;

        [TestInitialize]
        public void BeforeTest()
        {
            _appRepository = new InMemoryApplicationRepository();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
            _toggleScheduleRepository = new InMemoryRepository<ToggleSchedule>();
            _featureToggleController = new FeatureTogglesController(_appRepository, _httpContextAccessor, _toggleScheduleRepository);
            _toggleSchedulerConstroller = new ToggleSchedulerController(_toggleScheduleRepository, _appRepository, _httpContextAccessor);

        }

        [TestMethod]
        public async Task RemoveFeatureToggle_FeatureToggleIsDeleted()
        {
            //arrange
            var app = Application.Create("TestApp", "DEV", false, false, false);
            app.AddFeatureToggle("t1", "", "workItemId1");
            var theToggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);

            //act
            var result = await _featureToggleController.RemoveFeatureToggle(theToggle.Id, app.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _appRepository.FindByIdAsync(app.Id)).FeatureToggles.Count.Should().Be(0);
        }

        [TestMethod]
        public async Task RemoveFeatureToggle_SchedulersAreDeleted()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false, false, false);
            app.AddDeployEnvironment("QA", false, false, false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            var toggle = app.FeatureToggles.Single();
            await _appRepository.AddAsync(app);
            await _toggleSchedulerConstroller.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1" },
                Environments = new List<string> { "DEV", "QA" },
                ScheduleDate = date,
                State = true
            });

            //act
            var result = await _featureToggleController.RemoveFeatureToggle(toggle.Id, app.Id);


            //assert
            result.Should().BeOfType<OkResult>();
            var schedulers = await _toggleScheduleRepository.GetAllAsync();
            var forToggle = schedulers.Where(ft => ft.ToggleName == "t1").ToList();
            forToggle.Count().Should().Be(0);
        }
    }
}