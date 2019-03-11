using System.Threading.Tasks;
using MassTransit;
using MogglesClient.Logging;
using MogglesClient.PublicInterface;
using MogglesContracts;

namespace MogglesClient.Messaging.RefreshCache
{
    public class ClearTogglesCacheConsumer : IConsumer<RefreshTogglesCache>
    {
        private MogglesToggleService _featureToggleService;
        private IMogglesLoggingService _featureToggleLoggingService;
        private IMogglesConfigurationManager _mogglesConfigurationManager;

        public ClearTogglesCacheConsumer(){}
    
        public Task Consume(ConsumeContext<RefreshTogglesCache> context)
        {
            _featureToggleService = (MogglesToggleService)MogglesContainer.Resolve<MogglesToggleService>();
            _featureToggleLoggingService = (IMogglesLoggingService)MogglesContainer.Resolve<IMogglesLoggingService>();
            _mogglesConfigurationManager = (IMogglesConfigurationManager)MogglesContainer.Resolve<IMogglesConfigurationManager>();

            //This is a workaround for Ecommerce
            //For the case when the backend solution goes to sleep and MogglesClient is never restarted
#if NETFULL
            if (!InstancesAreCreated())
            {
                PublicInterface.MogglesClient.ConfigureAndStartClient();
            }
#endif

            var msg = context.Message;

            if (msg.ApplicationName.ToLowerInvariant() == _mogglesConfigurationManager.GetApplicationName().ToLowerInvariant() &&
                msg.Environment.ToLowerInvariant() == _mogglesConfigurationManager.GetEnvironment().ToLowerInvariant())
            {
                _featureToggleLoggingService.TrackEvent($"Handled cache refresh event for {msg.ApplicationName}/{msg.Environment}");
                _featureToggleService.CacheFeatureToggles();
            }

            return Task.FromResult(0);
        }

        private bool InstancesAreCreated()
        {
            return _mogglesConfigurationManager == null || _featureToggleService == null || _featureToggleLoggingService == null;
        }
    }
}