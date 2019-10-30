using System;
using System.Collections.Generic;
using System.Linq;

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
                ToggleName = name,
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
            FeatureToggleStatuses.Add(FeatureToggleStatus.Create(envName, enabled));
          
        }

        public void RemoveStatus(string environment)
        {
            FeatureToggleStatuses.RemoveAll(s => s.EnvironmentName == environment);
        }

        public void SetPermanentStatus(bool isPermanent)
        {
            IsPermanent = isPermanent;
        }

        public void SetNotes(string notes)
        {
            Notes = notes;
        }

        public void MarkAsAccepted()
        {
            UserAccepted = true;
        }

        public void MarkUserRejected()
        {
            UserAccepted = false;
        }

        public void ChangeName(string newName)
        {
            ToggleName = newName;
        }

        public void Toggle(string environment, bool isEnabled, string updatedBy)
        {
            var status = FeatureToggleStatuses.FirstOrDefault(s => s.EnvironmentName == environment);
            status.ToggleStatus(isEnabled, updatedBy);
        }

        public void MarkAsDeployed(string envName)
        {
            FeatureToggleStatuses.FirstOrDefault(fts=>fts.EnvironmentName==envName)?.MarkAsDeployed();
        }

        public void MarkAsNotDeployed(string envName)
        {
            FeatureToggleStatuses.FirstOrDefault(fts => fts.EnvironmentName == envName)?.MarkAsNotDeployed();

        }

        public void ChangeLastUpdateUsername(string envName, string updatedBy)
        {
            var featureToggleStatus = FeatureToggleStatuses.FirstOrDefault(fts => fts.EnvironmentName == envName);
            featureToggleStatus.ChangeLastUpdateUser(updatedBy);
        }
    }
}