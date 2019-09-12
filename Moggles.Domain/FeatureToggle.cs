using System;
using System.Collections.Generic;

namespace Moggles.Domain
{
    public class FeatureToggle : Entity
    {
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }
        public List<FeatureToggleStatus> FeatureToggleStatuses { get; set; } = new List<FeatureToggleStatus>();

        public static FeatureToggle Create(string name, string notes, bool isPermanent)
        {
            return new FeatureToggle
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                IsPermanent = isPermanent,
                Notes = notes,
                ToggleName = name
            };
        }

        public static FeatureToggle Create(string name, string notes, bool isPermanent, IEnumerable<DeployEnvironment> deployEnvironments)
        {
            var newToggle = Create(name, notes, isPermanent);

            foreach (var env in deployEnvironments)
            {
                newToggle.AddStatus(env.DefaultToggleValue, env.EnvName);
            }

            return newToggle;
        }

        public void AddStatus(bool enabled, string envName)
        {
            FeatureToggleStatuses.Add(new FeatureToggleStatus
            {
                Id = Guid.NewGuid(),
                Enabled = enabled,
                EnvironmentName = envName,
                LastUpdated = DateTime.UtcNow
            });
        }

        public void RemoveStatus(string environment)
        {
            FeatureToggleStatuses.RemoveAll(s => s.EnvironmentName == environment);
        }
    }
}