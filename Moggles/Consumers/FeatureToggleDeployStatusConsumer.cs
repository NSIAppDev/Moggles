using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MogglesContracts;
using Moggles.Data;
using Moggles.Domain;

namespace Moggles.Consumers
{
    public class FeatureToggleDeployStatusConsumer : IConsumer<RegisteredTogglesUpdate>
    {
        private IRepository<Application> _applicationsRepository;

        public FeatureToggleDeployStatusConsumer(IRepository<Application> applicationsRepository)
        {
            _applicationsRepository = applicationsRepository;
        }

        public async Task Consume(ConsumeContext<RegisteredTogglesUpdate> context)
        {
            string appName = context.Message.AppName;
            string envName = context.Message.Environment;
            string[] clientToggles = context.Message.FeatureToggles;

            var app = _applicationsRepository.GetAllAsync().Result.Where(a => a.AppName == appName).FirstOrDefault();

            var deployedToggles = app.FeatureToggles
                .Where(ft=> clientToggles.Contains(ft.ToggleName))
                .SelectMany(ft => ft.FeatureToggleStatuses)
                .Where(fts => fts.EnvironmentId == app.DeploymentEnvironments.FirstOrDefault(env=>env.EnvName == envName).Id).ToList();

            foreach (var featureToggle in deployedToggles)
            {
                featureToggle.MarkAsDeployed();
            }

            var removedToggles = app.FeatureToggles
                .Where(ft => !clientToggles.Contains(ft.ToggleName))
                .SelectMany(ft => ft.FeatureToggleStatuses)
                .Where(fts => fts.EnvironmentId == app.DeploymentEnvironments.FirstOrDefault(env => env.EnvName == envName).Id).ToList();
            foreach (var featureToggle in removedToggles)
            {
                featureToggle.MarkAsNotDeployed();
            }

            _applicationsRepository.UpdateAsync(app);
        }
    }
}


