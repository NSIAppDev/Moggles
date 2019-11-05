using System;
using System.Linq;
using Moggles.Domain;
using Moggles.Domain.DTO;

namespace Moggles.UnitTests.Helpers
{
    public static class FeatureToggleTestHelper
    {

        public static FeatureToggleStatusData GetFeatureToggleStatus(Application app, string toggleName, string environment)
        {
            var toggle = GetToggle(app, toggleName);
            var featureStatus = toggle.GetFeatureToggleStatusForEnv(environment);

            return new FeatureToggleStatusData
            {
                EnvironmentName = featureStatus.EnvironmentName,
                Enabled = featureStatus.Enabled,
                UpdatedBy = featureStatus.UpdatedbyUser
            };
        }

        public static void UpdateFeatureToggleDeployedStatus(Application app, string toggleName, string environment, bool deployedStatus)
        {
            var toggle = GetToggle(app, toggleName);
            var featureStatus = toggle.GetFeatureToggleStatusForEnv(environment);
            featureStatus.IsDeployed = deployedStatus;
        }

        public static bool GetFeatureToggleDeployedStatus(Application app, string toggleName, string environment)
        {
            var toggle = GetToggle(app, toggleName);
            var featureStatus = toggle.GetFeatureToggleStatusForEnv(environment);
            return featureStatus.IsDeployed;
        }

        private static FeatureToggle GetToggle(Application app, string toggleName)
        {
            return app.FeatureToggles.FirstOrDefault(f => string.Compare(f.ToggleName, toggleName, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
