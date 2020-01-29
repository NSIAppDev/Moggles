using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.UnitTests.ScheduleTogglesTests
{
    [TestClass]
    public class ToggleSchedulerControllerTests
    {
        private ToggleSchedulerController _sut;

        private IRepository<Application> _appRepository;
        private IRepository<ToggleSchedule> _toggleSchedulesRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;


        [TestInitialize]
        public void BeforeEach()
        {
            _appRepository = new InMemoryApplicationRepository();
            _toggleSchedulesRepository = new InMemoryRepository<ToggleSchedule>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("bla");
            _httpContextAccessor = _mockHttpContextAccessor.Object;
            _sut = new ToggleSchedulerController(_toggleSchedulesRepository, _appRepository, _httpContextAccessor);

        }

        [TestMethod]
        public async Task SchedulesAreCreatedForEachToggle()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);

            //act
            await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1", "t2" },
                Environments = new List<string> { "DEV", "QA" },
                ScheduleDate = date,
                State = true
            });

            //assert
            var schedules = await _toggleSchedulesRepository.GetAllAsync();
            var sc1 = schedules.FirstOrDefault(s => s.ToggleName == "t1");
            schedules.Count().Should().Be(2);
            sc1.ScheduledState.Should().BeTrue();
            sc1.Environments.Should().BeEquivalentTo(new[] { "DEV", "QA" });
            sc1.ApplicationName.Should().Be("tst");
            sc1.ScheduledDate.Should().Be(date.ToUniversalTime());
        }

        [TestMethod]
        public async Task ReturnBadRequestResult_WhenModelStateIsInvalid()
        {
            //arrange
            _sut.ModelState.AddModelError("error", "some error");

            //act
            var result = await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = Guid.NewGuid()
            });

            //assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ThrowsException_WhenAppIdIsInvalid()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            await _appRepository.AddAsync(app);

            //act
            Func<Task> act = async () => await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = Guid.Empty
            });

            //assert
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public async Task ReturnScheduleToggles_WhenThereAreTogglesScheduled()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);
            await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1", "t2" },
                Environments = new List<string> { "DEV", "QA" },
                ScheduleDate = date,
                State = true
            });

            //act
            var apps = await _sut.GetScheduledToggles(app.Id) as OkObjectResult;
            var appsList = apps.Value as IEnumerable<ToggleSchedule>;

            //assert
            appsList.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task ReturnBadResult_WhenNoTogglesAreScheduled()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);


            //act
            var apps = await _sut.GetScheduledToggles(app.Id) as OkObjectResult;
            var appsList = apps.Value as IEnumerable<ToggleSchedule>;


            //assert
            appsList.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task UpdateScheduledFeatureToggle_WithValidData()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);
            await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1", "t2" },
                Environments = new List<string> { "DEV" },
                ScheduleDate = date,
                State = true
            });
            var scheduledToggles = await _toggleSchedulesRepository.GetAllAsync();
            var scheduled = scheduledToggles.ToList().First();
            scheduled.Environments.Add("QA");

            //act
            var apps = await _sut.Update(new UpdateFeatureToggleSchedulerModel
            {
                Id = scheduled.Id,
                ToggleName = scheduled.ToggleName,
                Environments = scheduled.Environments,
                ScheduledDate = new DateTime(2020, 5, 2, 15, 45, 0),
                ScheduledState = false
            });

            //assert
            var updatedScheduledToggle = await _toggleSchedulesRepository.FindByIdAsync(scheduled.Id);
            updatedScheduledToggle.ToggleName.Should().Be("t1");
            updatedScheduledToggle.Environments.Should().BeEquivalentTo(new List<string> { "DEV", "QA" });
            updatedScheduledToggle.ScheduledDate.Should().Be(new DateTime(2020, 5, 2, 15, 45, 0));
            updatedScheduledToggle.ScheduledState.Should().BeFalse();
        }

        [TestMethod]
        public async Task DeleteToggleScheduler_SchedulerIsDeleted() 
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);
            await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1", "t2" },
                Environments = new List<string> { "DEV" },
                ScheduleDate = date,
                State = true
            });
            var scheduledToggles = await _toggleSchedulesRepository.GetAllAsync();
            var scheduled = scheduledToggles.ToList().First();

            //act
            var result = await _sut.Delete(scheduled.Id);

            //assert
            result.Should().BeOfType<OkResult>();
            (await _toggleSchedulesRepository.GetAllAsync()).Count().Should().Be(1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task DeteleToggleScheduler_WithInvalidId_ThrowsInvalidOperationException()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null, "workItemId1");
            app.AddFeatureToggle("t2", null, "workItemId2");
            await _appRepository.AddAsync(app);
            await _sut.ScheduleToggles(new ScheduleTogglesModel
            {
                ApplicationId = app.Id,
                FeatureToggles = new List<string> { "t1", "t2" },
                Environments = new List<string> { "DEV" },
                ScheduleDate = date,
                State = true
            });
            var scheduledToggles = await _toggleSchedulesRepository.GetAllAsync();
            var scheduled = scheduledToggles.ToList().First();

            //act
            var result = await _sut.Delete(Guid.NewGuid());

            //assert
        }
        
    }
}