using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.BackgroundServices;
using Moggles.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moggles.UnitTests.Helpers;
using Moggles.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Moggles.UnitTests.ScheduleTogglesTests
{
    [TestClass]
    public class ScheduledFeatureTogglesServiceTests
    {
        private IRepository<Application> _appRepository;
        private IRepository<ToggleSchedule> _toggleSchedulesRepository;
        private ScheduledFeatureTogglesService _sut;
        private CancellationTokenSource _cts;
        private readonly DateTime _dateInThePast = new DateTime(2018,1,1,15,30,0);
        private Mock<IIsDueHub> _hubContext;
        private Mock<IHubContext<IsDueHub, IIsDueHub>> _hubContextMock;
        private Mock<ILogger<ScheduledFeatureTogglesService>> _loggerMock;

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

            _hubContextMock = new Mock<IHubContext<IsDueHub, IIsDueHub>>();
            var hubCltMock = new Mock<IHubClients<IIsDueHub>>();

            _hubContext = new Mock<IIsDueHub>();
       
            hubCltMock.Setup(_ => _.All).Returns(_hubContext.Object);
            _hubContextMock.Setup(_ => _.Clients).Returns(hubCltMock.Object);
            _loggerMock = new Mock<ILogger<ScheduledFeatureTogglesService>>();

            _sut = new ScheduledFeatureTogglesService(_loggerMock.Object, serviceProvider, _hubContextMock.Object);
            _cts = new CancellationTokenSource();
        }


        [TestMethod]
        public async Task FlipToggles_HavingSheduledTimeInThePast()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddFeatureToggle("offToggle", null);
            app.AddFeatureToggle("onToggle", null);
            app.SetToggle(app.FeatureToggles.Single(f => f.ToggleName == "offToggle").Id, "DEV", false, "username");
            app.SetToggle(app.FeatureToggles.Single(f => f.ToggleName == "onToggle").Id, "DEV", true, "username");
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst","offToggle", new[] { "DEV" }, true, _dateInThePast, "updatedBy");
            var schedule2 = ToggleSchedule.Create("tst","onToggle", new[] { "DEV" }, false, _dateInThePast, "updatedBy");
            await _toggleSchedulesRepository.AddAsync(schedule);
            await _toggleSchedulesRepository.AddAsync(schedule2);

            //act
            await _sut.StartAsync(_cts.Token);
            await _sut.StopAsync(_cts.Token);

            //assert
            var updatedApp = await _appRepository.FindByIdAsync(app.Id);
            var status = FeatureToggleTestHelper.GetFeatureToggleStatus(updatedApp, "offtoggle", "DEV");
            status.Enabled.Should().BeTrue();
            var status2 = FeatureToggleTestHelper.GetFeatureToggleStatus(updatedApp, "onToggle", "DEV");
            status2.Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task OnceToggleIsSet_TheScheduleIsRemoved()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            app.AddFeatureToggle("t1", null);
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst", "t1", new[] { "DEV" }, true, _dateInThePast, "updatedBy");
            await _toggleSchedulesRepository.AddAsync(schedule);

            //act
            await _sut.StartAsync(_cts.Token);
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
            await _sut.StopAsync(_cts.Token);

            //assert
            var updatedApp = await _appRepository.FindByIdAsync(app.Id);
            var status = FeatureToggleTestHelper.GetFeatureToggleStatus(updatedApp, "t1", "DEV");
            status.Enabled.Should().BeFalse();
        }

        [TestMethod]
        public async Task IfFeatureToggleIsDeleted_BeforeScheduledDate_RemovesTheSchedule()
        {
            //arrange
            var app = Application.Create("tst", "DEV", false);
            await _appRepository.AddAsync(app);

            var schedule = ToggleSchedule.Create("tst", "DeletedToggle", new[] { "DEV" }, true, _dateInThePast, "updatedBy");
            await _toggleSchedulesRepository.AddAsync(schedule);

            //act
            await _sut.StartAsync(_cts.Token);
            await _sut.StopAsync(_cts.Token);

            //assert
            (await _toggleSchedulesRepository.GetAllAsync()).Count().Should().Be(0);
        }
    }
}