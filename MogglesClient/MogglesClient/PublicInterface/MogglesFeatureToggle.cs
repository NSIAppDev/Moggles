using System.Linq;

namespace MogglesClient.PublicInterface
{
    public class MogglesFeatureToggle
    {
        private readonly string _name;

#if NETFULL
        static MogglesFeatureToggle()
        {
            MogglesClient.ConfigureAndStartClient();
        }
#endif

        public MogglesFeatureToggle()
        {
            _name = GetType().Name;
        }

        public MogglesFeatureToggle(string toggleName)
        {
            _name = toggleName;
        }

        public bool IsEnabled => IsFeatureToggleEnabled();

        private bool IsFeatureToggleEnabled()
        {
            var featureToggleService = (MogglesToggleService)MogglesContainer.Resolve<MogglesToggleService>();
            var configurationManager = (IMogglesConfigurationManager)MogglesContainer.Resolve<IMogglesConfigurationManager>();

            if (configurationManager.IsApplicationInTestingMode())
            {
                return configurationManager.GetFeatureToggleValueFromConfig(_name);
            }

            var featureToggleValue = featureToggleService.GetFeatureTogglesFromCache()
                ?.FirstOrDefault(x => x.FeatureToggleName == _name);

            return featureToggleValue?.IsEnabled ?? false;
        }

    }
}
