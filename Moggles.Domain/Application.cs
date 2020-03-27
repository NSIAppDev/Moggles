using System;
using System.Collections.Generic;
using System.Linq;
using Moggles.Domain.DTO;

namespace Moggles.Domain
{
    public class Application : AggregateRoot
    {
        public string AppName { get; set; }

        public List<DeployEnvironment> DeploymentEnvironments { get; set; } = new List<DeployEnvironment>();
        public List<FeatureToggle> FeatureToggles { get; set; } = new List<FeatureToggle>();

        public void UpdateName(string newName)
        {
            AppName = newName;
        }

        public void AddDeployEnvironment(string name, bool defaultToggleValue, bool requireReasonWhenToggleEnabled, bool requireReasonWhenToggleDisabled, int sortOrder = 1)
        {
            if (DeployEnvExists(name))
                throw new BusinessRuleValidationException("Environment with the same name already exists for this application!");

            DeploymentEnvironments.Add(DeployEnvironment.Create(name, defaultToggleValue, requireReasonWhenToggleEnabled, requireReasonWhenToggleDisabled, sortOrder));

            foreach (var ft in FeatureToggles)
            {
                ft.AddStatus(defaultToggleValue, name);
            }
        }

        private bool DeployEnvExists(string newName, string oldName = "")
        {
            return (DeploymentEnvironments.Exists(e => string.Compare(e.EnvName, newName, StringComparison.OrdinalIgnoreCase) == 0) && newName != oldName);
        }

        public static Application Create(string appName, string defaultEnvironmentName, bool defaultToggleValueForEnvironment)
        {
            var app = new Application
            {
                Id = Guid.NewGuid(),
                AppName = appName
            };
            app.AddDeployEnvironment(defaultEnvironmentName, defaultToggleValueForEnvironment, false, false);
            return app;
        }

        public void AddFeatureToggle(string toggleName, string notes, string workItemIdentifier, bool isPermanent = false)
        {
            if (FeatureToggles.Exists(f => string.Compare(f.ToggleName, toggleName, StringComparison.OrdinalIgnoreCase) == 0))
            {
                throw new BusinessRuleValidationException("Feature toggle with the same name already exists for this application!");
            }

            var ft = FeatureToggle.Create(toggleName, notes, isPermanent, DeploymentEnvironments, workItemIdentifier);
            FeatureToggles.Add(ft);
        }

        public void DeleteDeployEnvironment(string environment)
        {
            if (!DeployEnvExists(environment))
            {
                throw new InvalidOperationException("Environment does not exist!");
            }

            foreach (var featureToggle in FeatureToggles)
            {
                featureToggle.RemoveStatus(environment);
            }

            DeploymentEnvironments.RemoveAll(e => e.EnvName == environment);
        }

        public void ChangeDeployEnvironmentName(string oldName, string newName)
        {
            var env = DeploymentEnvironments.FirstOrDefault(e => string.Compare(e.EnvName, oldName, StringComparison.OrdinalIgnoreCase) == 0);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            if (DeployEnvExists(newName, oldName))
            {
                throw new BusinessRuleValidationException("An environment with the same name already exists!");
            }

            env.EnvName = newName;

            var featureToggles = FeatureToggles.ToList();
            foreach (var ft in featureToggles)
            {
                ft.ChangeEnvironmentnameForFeatureToggleStatus(oldName, newName);
            }

        }

        public void ChangeEnvironmentValuesToRequireReasonFor(string name, bool requireReasonWhenToggleEnabled, bool requireReasonWhenToggleDisabled)
        {
            var env = DeploymentEnvironments.FirstOrDefault(e => string.Compare(e.EnvName, name, StringComparison.OrdinalIgnoreCase) == 0);

            if (env == null)
            {
                throw new InvalidOperationException("Environment does not exist!");
            }

            env.RequireReasonWhenToggleDisabled = requireReasonWhenToggleDisabled;
            env.RequireReasonWhenToggleEnabled = requireReasonWhenToggleEnabled;
        }
        public void ChangeEnvironmentDefaultValue(string name, bool newDefaultValue)
        {
            var env = DeploymentEnvironments.FirstOrDefault(e => string.Compare(e.EnvName, name, StringComparison.OrdinalIgnoreCase) == 0);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            env.DefaultToggleValue = newDefaultValue;
        }

        public void ChangeEnvironmentPosition(string environment, bool moveToLeft, bool moveToRight)
        {
            DeploymentEnvironments = DeploymentEnvironments.OrderBy(e => e.SortOrder).ToList();
            var env = DeploymentEnvironments.FirstOrDefault(e => string.Compare(e.EnvName, environment, StringComparison.OrdinalIgnoreCase) == 0);

            if (env == null)
                throw new InvalidOperationException("Environment does not exists!");

            if (moveToLeft && !moveToRight)
            {
                MoveLeft(env);
            }

            if(moveToRight && !moveToLeft)
            {
                MoveRight(env);
            }
        }
        private void MoveLeft(DeployEnvironment environment)
        {
            var index = DeploymentEnvironments.IndexOf(environment);
            if (index > 0)
            {
                var leftEnvironment = DeploymentEnvironments[index - 1];
                var sortOrder = environment.SortOrder;
                environment.SortOrder = leftEnvironment.SortOrder;
                leftEnvironment.SortOrder = sortOrder;
            }
        }

        private void MoveRight(DeployEnvironment environment)
        {
            var index = DeploymentEnvironments.IndexOf(environment);
            if (index < DeploymentEnvironments.Count - 1) 
            {
                var rightEnvironment = DeploymentEnvironments[index +1 ];
                var sortOrder = environment.SortOrder;
                environment.SortOrder = rightEnvironment.SortOrder;
                rightEnvironment.SortOrder = sortOrder;
            }
        }

        public void RemoveFeatureToggle(Guid id)
        {
            FeatureToggles.RemoveAll(t => t.Id == id);
        }

        public ToggleData GetFeatureToggleBasicData(Guid toggleId)
        {
            var toggle = GuardToggleExists(toggleId);

            return new ToggleData
            {
                ToggleName = toggle.ToggleName,
                IsPermanent = toggle.IsPermanent,
                Notes = toggle.Notes,
                UserAccepted = toggle.UserAccepted,
                WorkItemIdentifier = toggle.WorkItemIdentifier,
                ReasonsToChanges = toggle.ReasonsToChange
            };
        }

        public void UpdateFeatureToggleReasonsToChange(Guid toggleId, string addedByUser, string description)
        {
            var toggle = FeatureToggles.Find(ft => ft.Id == toggleId);
            toggle.AddReasonToChange(addedByUser, description);
        }

        public void UpdateFeatureTogglePermanentStatus(Guid toggleId, bool isPermanent)
        {
            var toggle = FeatureToggles.Find(f => f.Id == toggleId);
            toggle.SetPermanentStatus(isPermanent);
        }

        public void UpdateFeatureToggleNotes(Guid toggleId, string notes)
        {
            var toggle = FeatureToggles.Find(f => f.Id == toggleId);
            toggle.SetNotes(notes);
        }
        public void UpdateFeaturetoggleWorkItemIdentifier(Guid toggleId, string workItemIdentifier)
        {
            var toggle = FeatureToggles.Find(ft => ft.Id == toggleId);
            toggle.SetWorkItemIdentifier(workItemIdentifier);
        }

        public void FeatureAcceptedByUser(Guid toggleId)
        {
            var toggle = FeatureToggles.Find(f => f.Id == toggleId);
            toggle.MarkAsAccepted();
        }

        public void FeatureRejectedByUser(Guid toggleId)
        {
            var toggle = FeatureToggles.Find(f => f.Id == toggleId);
            toggle.MarkUserRejected();
        }

        public void ChangeFeatureToggleName(Guid toggleId, string newName)
        {
            if (FeatureToggles.Exists(t => string.Compare(t.ToggleName, newName, StringComparison.OrdinalIgnoreCase) == 0 && t.Id != toggleId))
            {
                throw new BusinessRuleValidationException($"There is already a feature toggle with the name {newName}");
            }

            var toggle = FeatureToggles.Find(f => f.Id == toggleId);
            toggle.ChangeName(newName);
        }

        public List<FeatureToggleStatusData> GetFeatureToggleStatuses(Guid toggleId)
        {
            var toggle = GuardToggleExists(toggleId);

            return toggle.FeatureToggleStatuses.Select(x => new FeatureToggleStatusData
            {
                EnvironmentName = x.EnvironmentName,
                Enabled = x.Enabled,
                UpdatedBy = x.UpdatedbyUser
            }).ToList();
        }

        public void SetToggle(Guid toggleId, string environment, bool isEnabled, string updatedBy)
        {
            var toggle = GuardToggleExists(toggleId);
            toggle.Toggle(environment, isEnabled, updatedBy);
        }

        public void SetToggle(string name, string environment, bool isEnabled, string updatedBy)
        {
            var toggle = GuardToggleExists(name);
            toggle.Toggle(environment, isEnabled, updatedBy);
        }

        private FeatureToggle GuardToggleExists(Guid toggleId)
        {
            var toggle = FeatureToggles.FirstOrDefault(f => f.Id == toggleId);
            if (toggle is null)
                throw new EntityNotFoundException("Feature toggle not found!", typeof(FeatureToggle).Name);
            return toggle;
        }

        private FeatureToggle GuardToggleExists(string toggleName)
        {
            var toggle = FeatureToggles.FirstOrDefault(f => f.ToggleName == toggleName);
            if (toggle is null)
                throw new EntityNotFoundException("Feature toggle not found!", typeof(FeatureToggle).Name);
            return toggle;
        }

        public void MarkDeployedFeatureToggles(string[] clientToggles, string envName)
        {
            List<FeatureToggle> deployedToggles = FeatureToggles.Where(ft => clientToggles.Contains(ft.ToggleName)).ToList();

            foreach (var featureToggle in deployedToggles)
            {
                featureToggle.MarkAsDeployed(envName);
            }

            List<FeatureToggle> removedToggles = FeatureToggles.Where(ft => !clientToggles.Contains(ft.ToggleName)).ToList();
            foreach (var featureToggle in removedToggles)
            {
                featureToggle.MarkAsNotDeployed(envName);
            }
        }
    }
}