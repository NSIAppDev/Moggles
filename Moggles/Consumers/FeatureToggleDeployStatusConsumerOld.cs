using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSTogglesClient;
using Moggles.Data;

namespace Moggles.Consumers
{
    public class FeatureToggleDeployStatusConsumerOld : IConsumer<RegisteredTogglesUpdate>
    {
        private readonly TogglesContext _db;

        public FeatureToggleDeployStatusConsumerOld(TogglesContext db)
        {
            _db = db;
        }

        public async Task Consume(ConsumeContext<RegisteredTogglesUpdate> context)
        {
            string appName = context.Message.AppName;
            string envName = context.Message.Environment;
            string[] clientToggles = context.Message.FeatureToggles;

            var featureToggles = _db.FeatureToggles.Include(ft => ft.Application).Include(ft => ft.FeatureToggleStatuses).ThenInclude(fts => fts.Environment)
                .Where(ft => ft.Application.AppName == appName)
                .ToList();

            var deployedToggles = featureToggles.Where(ft => clientToggles.Contains(ft.ToggleName)).ToList();
            foreach (var featureToggle in deployedToggles)
            {
                featureToggle.MarkAsDeployed(envName);
            }

            var removedToggles = featureToggles.Where(ft => !clientToggles.Contains(ft.ToggleName)).ToList();
            foreach (var featureToggle in removedToggles)
            {
                featureToggle.MarkAsNotDeployed(envName);
            }
            await _db.SaveChangesAsync();
        }
    }
}


