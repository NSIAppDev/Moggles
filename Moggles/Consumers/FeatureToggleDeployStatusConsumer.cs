using System;
using MassTransit;
using Moggles.Domain;
using MogglesContracts;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Consumers
{
    public class FeatureToggleDeployStatusConsumer : IConsumer<RegisteredTogglesUpdate>
    {
        private readonly IRepository<Application> _applicationsRepository;

        public FeatureToggleDeployStatusConsumer(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        public async Task Consume(ConsumeContext<RegisteredTogglesUpdate> context)
        {
            string appName = context.Message.AppName;
            string envName = context.Message.Environment;
            string[] clientToggles = context.Message.FeatureToggles;

            var apps = await _applicationsRepository.GetAllAsync();
            var app = apps.FirstOrDefault(a => string.Compare(a.AppName, appName, StringComparison.OrdinalIgnoreCase) == 0);

            if (app != null)
            {
                app.MarkDeployedFeatureToggles(clientToggles, envName);

                await _applicationsRepository.UpdateAsync(app);
            }
        }
    }
}


