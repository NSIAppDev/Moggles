﻿using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.BackgroundServices;
using Moggles.Domain;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Moggles.UnitTests.ScheduleTogglesTests
{
    [TestClass]
    public class ScheduledFeatureTogglesServiceTests
    {
        private IRepository<Application> _appRepository;
        private IRepository<ToggleSchedule> _toggleSchedulesRepository;
        private ScheduledFeatureTogglesService _sut;
        private CancellationTokenSource _cts;

        [TestInitialize]
        public void BeforeEach()
        {
            _appRepository = new InMemoryApplicationRepository();
            _toggleSchedulesRepository = new InMemoryRepository<ToggleSchedule>();
            var services = new ServiceCollection();
            services.AddScoped(sp => _appRepository);
            services.AddScoped(sp => _toggleSchedulesRepository);
            services.AddLogging(cfg => cfg.AddConsole()).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Trace);
            var serviceProvider = services.BuildServiceProvider();
            _sut = new ScheduledFeatureTogglesService(serviceProvider.GetService<ILogger<ScheduledFeatureTogglesService>>(), serviceProvider);
            _cts = new CancellationTokenSource();
        }


        [TestMethod]
        public async Task FlipToggles_HavingSheduledTimeInThePast()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddFeatureToggle("offToggle", null);
            app.AddFeatureToggle("onToggle", null);
            app.SetToggle(app.FeatureToggles.Single(f => f.ToggleName == "offToggle").Id, "DEV", false);
            app.SetToggle(app.FeatureToggles.Single(f => f.ToggleName == "onToggle").Id, "DEV", true);
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst","offToggle", new[] { "DEV" }, true, DateTime.UtcNow);
            var schedule2 = ToggleSchedule.Create("tst","onToggle", new[] { "DEV" }, false, DateTime.UtcNow);
            await _toggleSchedulesRepository.AddAsync(schedule);
            await _toggleSchedulesRepository.AddAsync(schedule2);

            //act
            await _sut.StartAsync(_cts.Token);
            Thread.Sleep(1000);
            Trace.WriteLine("finished sleeping");
            await _sut.StopAsync(_cts.Token);

            //assert
            var updatedApp = await _appRepository.FindByIdAsync(app.Id);
            var status = updatedApp.GetFeatureToggleStatus("offtoggle", "DEV");
            status.Enabled.Should().BeTrue();
            var status2 = updatedApp.GetFeatureToggleStatus("onToggle", "DEV");
            status2.Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnceToggleIsSet_TheScheduleIsRemoved()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddFeatureToggle("t1", null);
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst", "t1", new[] { "DEV" }, true, DateTime.UtcNow);
            await _toggleSchedulesRepository.AddAsync(schedule);

            //act
            await _sut.StartAsync(_cts.Token);
            Thread.Sleep(1000);
            Trace.WriteLine("finished sleeping");
            await _sut.StopAsync(_cts.Token);

            //assert
            (await _toggleSchedulesRepository.GetAllAsync()).Count().Should().Be(0);
        }

        [TestMethod]
        public async Task WhenThereAreNoSchedulesDoNothing()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddFeatureToggle("t1", null);
            await _appRepository.AddAsync(app);

            //act
            await _sut.StartAsync(_cts.Token);
            Thread.Sleep(1000);
            Trace.WriteLine("finished sleeping");
            await _sut.StopAsync(_cts.Token);

            //assert
            var updatedApp = await _appRepository.FindByIdAsync(app.Id);
            var status = updatedApp.GetFeatureToggleStatus("t1", "DEV");
            status.Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task IfFeatureToggleIsDeleted_BeforeScheduledDate_RemovesTheSchedule()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst", "DeletedToggle", new[] { "DEV" }, true, DateTime.UtcNow);
            await _toggleSchedulesRepository.AddAsync(schedule);

            //act
            await _sut.StartAsync(_cts.Token);
            Thread.Sleep(1000);
            Trace.WriteLine("finished sleeping");
            await _sut.StopAsync(_cts.Token);

            //assert
            (await _toggleSchedulesRepository.GetAllAsync()).Count().Should().Be(0);
        }
    }
}