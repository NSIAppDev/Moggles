using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.Controllers;
using Moggles.Domain;
using Moggles.Models;
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

        [TestInitialize]
        public void BeforeEach()
        {
            _appRepository = new InMemoryApplicationRepository();
            _toggleSchedulesRepository = new InMemoryRepository<ToggleSchedule>();
            _sut = new ToggleSchedulerController(_toggleSchedulesRepository, _appRepository);
        }

        [TestMethod]
        public async Task SchedulesAreCreatedForEachToggle()
        {
            //arrange
            var date = new DateTime(2099, 3, 2, 15, 45, 0);
            var app = Application.Create("tst", "DEV", false);
            app.AddDeployEnvironment("QA", false);
            app.AddFeatureToggle("t1", null);
            app.AddFeatureToggle("t2", null);
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
    }
}