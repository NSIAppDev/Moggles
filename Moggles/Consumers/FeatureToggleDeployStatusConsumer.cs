using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using MogglesContracts;
using Moggles.Data;
using Moggles.Domain;
using Moggles.Repository;

namespace Moggles.Consumers
{
    public class FeatureToggleDeployStatusConsumer : IConsumer<RegisteredTogglesUpdate>
    {
        private IRepository<Application> _applicationsRepository;
        private IRepository<DeployEnvironment> _deployEnvironmentRepository;
        private IRepository<FeatureToggle> _featureToggleRepository;
        private IRepository<FeatureToggleStatus> _featureToggleStatusRepository;

        public FeatureToggleDeployStatusConsumer(IRepository<Application> applicationsRepository, IRepository<DeployEnvironment> deployEnvironmentRepository, IRepository<FeatureToggle> featureToggleRepository, IRepository<FeatureToggleStatus> featureToggleStatusRepository)
        {
            _applicationsRepository = applicationsRepository;
            _deployEnvironmentRepository = deployEnvironmentRepository;
            _featureToggleRepository = featureToggleRepository;
            _featureToggleStatusRepository = featureToggleStatusRepository;
        }

        public async Task Consume(ConsumeContext<RegisteredTogglesUpdate> context)
        {
            string appName = context.Message.AppName;
            string envName = context.Message.Environment;
            string[] clientToggles = context.Message.FeatureToggles;

            var applicationId = _applicationsRepository.GetAll().Result.Where(app => app.AppName == appName).Select(app => app.Id).FirstOrDefault();
            var environmentId = _deployEnvironmentRepository.GetAll().Result.Where(env => env.EnvName == envName).Where(env => env.ApplicationId == applicationId).Select(env => env.Id).FirstOrDefault();

            var featureToggleStatuses = _featureToggleStatusRepository.GetAll().Result
                .Join(_featureToggleRepository.GetAll().Result.Where(x => x.ApplicationId == applicationId), fts => fts.FeatureToggleId, ft => ft.Id, (fts, ft) => new { FeatureToggleStatus = fts, FeatureToggle = ft })
                .Where(fts => fts.FeatureToggleStatus.EnvironmentId==environmentId);


            var deployedToggles = featureToggleStatuses.Where(ft => clientToggles.Contains(ft.FeatureToggle.ToggleName)).ToList();
            foreach (var featureToggle in deployedToggles)
            {
                featureToggle.FeatureToggleStatus.MarkAsDeployed();
                _featureToggleStatusRepository.Update(featureToggle.FeatureToggleStatus);
            }

            var removedToggles = featureToggleStatuses.Where(ft => !clientToggles.Contains(ft.FeatureToggle.ToggleName)).ToList();
            foreach (var featureToggle in removedToggles)
            {
                featureToggle.FeatureToggleStatus.MarkAsNotDeployed();
                _featureToggleStatusRepository.Update(featureToggle.FeatureToggleStatus);
            }
        }
    }
}


