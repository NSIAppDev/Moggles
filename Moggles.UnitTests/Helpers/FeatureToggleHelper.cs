using System;
using System.Linq;
using Moggles.Domain;
using Moggles.Domain.DTO;

namespace Moggles.UnitTests.Helpers
{
    public static class FeatureToggleHelper
    {

        public static FeatureToggleStatusData GetFeatureToggleStatus(Application app, string toggleName, string environment)
        {
            var toggle = app.FeatureToggles.FirstOrDefault(f => string.Compare(f.ToggleName, toggleName, StringComparison.OrdinalIgnoreCase) == 0);
            var featureStatus = toggle.GetFeatureToggleStatusForEnv(environment);

            return new FeatureToggleStatusData
            {
                EnvironmentName = featureStatus.EnvironmentName,
                Enabled = featureStatus.Enabled,
                UpdatedBy = featureStatus.UpdatedbyUser
            };
        }
    }
}
