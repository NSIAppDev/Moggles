using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moggles.Consumers;
using Moggles.Domain;
using Moggles.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Moggles.BackgroundServices
{
    public class ScheduledFeatureTogglesService : BackgroundService
    {
        private readonly ILogger<ScheduledFeatureTogglesService> _logger;
        private IRepository<Application> _appRepository;
        private IRepository<ToggleSchedule> _toggleSchedulesRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<IsDueHub, IIsDueHub> _hubContext;

        public ScheduledFeatureTogglesService(ILogger<ScheduledFeatureTogglesService> logger, IServiceProvider serviceProvider, IHubContext<IsDueHub, IIsDueHub> hubContext)
        {
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation($"ScheduledFeatureTogglesService background task is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation($" ScheduledFeatureTogglesService background task is stopping (via token)."));
            do
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        _appRepository = scope.ServiceProvider.GetRequiredService<IRepository<Application>>();
                        _toggleSchedulesRepository = scope.ServiceProvider.GetRequiredService<IRepository<ToggleSchedule>>();

                        _logger.LogDebug($"ScheduledFeatureTogglesService task doing background work.");
                        IEnumerable<ToggleSchedule> allSchedules = (await _toggleSchedulesRepository.GetAllAsync()).ToList();
                        if (allSchedules.Any())
                        {
                            var apps = await _appRepository.GetAllAsync();
                            _logger.LogDebug($"Schedule count = {allSchedules.Count()}");

                            foreach (var toggleSchedule in allSchedules)
                            {
                                _logger.LogDebug($"Schedule processing: {toggleSchedule.ScheduledDate}");
                                if (toggleSchedule.IsDue())
                                {
                                    var app = apps.FirstOrDefault(a => a.AppName == toggleSchedule.ApplicationName);
                                    foreach (var env in toggleSchedule.Environments)
                                    {
                                        try
                                        {
                                            app.SetToggle(toggleSchedule.ToggleName, env, toggleSchedule.ScheduledState, "Scheduled on behalf of "+toggleSchedule.UpdatedBy);
                                            _logger.LogInformation(
                                                $"Set toggle {toggleSchedule.ToggleName} to {toggleSchedule.ScheduledState} on {env} environment for {app.AppName}");

                                        }
                                        catch (EntityNotFoundException ex) when (ex.EntityType == nameof(FeatureToggle))
                                        {
                                            _logger.LogError(ex, ex.Message);
                                            await _toggleSchedulesRepository.DeleteAsync(toggleSchedule);
                                        }
                                    }
                                    await _appRepository.UpdateAsync(app);
                                    _hubContext.NotifyClient(toggleSchedule);
                                    await _toggleSchedulesRepository.DeleteAsync(toggleSchedule);
                                }
                                else
                                {
                                    _logger.LogDebug($"Schedule is not DUE: {toggleSchedule.ScheduledDate}, now={DateTime.Now}");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogDebug("No schedules found to apply.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, ex.Message);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }
    }
}