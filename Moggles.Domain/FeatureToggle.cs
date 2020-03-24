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
        public string WorkItemIdentifier { get; set; }
        public List<ReasonToChange> ReasonsToChange { get; set; } = new List<ReasonToChange>();

        public static FeatureToggle Create(string name, string notes, bool isPermanent, string workItemIdentifier)
        {
            return new FeatureToggle
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                IsPermanent = isPermanent,
                Notes = notes,
                ToggleName = name,
                WorkItemIdentifier = workItemIdentifier
            };
        }

        public static FeatureToggle Create(string name, string notes, bool isPermanent, IEnumerable<DeployEnvironment> deployEnvironments, string workItemIdentifier)
        {
            var newToggle = Create(name, notes, isPermanent, workItemIdentifier);

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
            var envStatus = GetFeatureToggleStatusForEnv(environment);
            FeatureToggleStatuses.Remove(envStatus);
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
            GetFeatureToggleStatusForEnv(environment)?.ToggleStatus(isEnabled, updatedBy);
        }

        public void MarkAsDeployed(string envName)
        {
            GetFeatureToggleStatusForEnv(envName)?.MarkAsDeployed();
        }

        public void MarkAsNotDeployed(string envName)
        {
            GetFeatureToggleStatusForEnv(envName)?.MarkAsNotDeployed();
        }

        public FeatureToggleStatus GetFeatureToggleStatusForEnv(string envName)
        {
            return FeatureToggleStatuses.FirstOrDefault(fts =>
                string.Compare(fts.EnvironmentName, envName, StringComparison.OrdinalIgnoreCase) == 0);
        }
        public void ChangeEnvironmentnameForFeatureToggleStatus(string oldEnvName, string NewEnvName)
        {
            var featureToggleStatuses = FeatureToggleStatuses.Where(fts => fts.EnvironmentName == oldEnvName);
            foreach(var fts in featureToggleStatuses)
            {
                fts.ChangeEnvironmentName(NewEnvName);
            }
        }

        public void SetWorkItemIdentifier(string workItemIdentifier)
        {
            WorkItemIdentifier = workItemIdentifier;
        }

        public void AddReasonToChange(string addedByUser, string description, List<string> environments)
        {
            ReasonsToChange.Add(ReasonToChange.Create(addedByUser, description, environments));
        }

    }
}