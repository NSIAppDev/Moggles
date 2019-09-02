using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MogglesContracts;
using Moggles.Domain;
using Moggles.Repository;

namespace Moggles.Consumers
{
    public class FeatureToggleDeployStatusConsumer : IConsumer<RegisteredTogglesUpdate>
    {
        private IRepository<FeatureToggle> _featureToggleRepository;

        public FeatureToggleDeployStatusConsumer(IRepository<FeatureToggle> featureToggleRepository)
        {
            _featureToggleRepository = featureToggleRepository;
        }

        public async Task Consume(ConsumeContext<RegisteredTogglesUpdate> context)
        {
            string appName = context.Message.AppName;
            string envName = context.Message.Environment;
            string[] clientToggles = context.Message.FeatureToggles;

            var featureToggles = _featureToggleRepository.GetAll().Result.AsQueryable().Include(ft => ft.Application).Include(ft => ft.FeatureToggleStatuses).ThenInclude(fts => fts.Environment)
                .Where(ft => ft.Application.AppName == appName)
                .ToList();

            var deployedToggles = featureToggles.Where(ft => clientToggles.Contains(ft.ToggleName)).ToList();
            foreach (var featureToggle in deployedToggles)
            {
                featureToggle.MarkAsDeployed(envName);
                _featureToggleRepository.Update(featureToggle);
            }

            var removedToggles = featureToggles.Where(ft => !clientToggles.Contains(ft.ToggleName)).ToList();
            foreach (var featureToggle in removedToggles)
            {
                featureToggle.MarkAsNotDeployed(envName);
                _featureToggleRepository.Update(featureToggle);
            }
        }
    }
}


