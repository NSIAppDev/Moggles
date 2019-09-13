﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.Domain
{
    public class Application : Entity, IAggregateRoot
    {
        public string AppName { get; set; }

        public List<DeployEnvironment> DeploymentEnvironments { get; set; } = new List<DeployEnvironment>();
        public List<FeatureToggle> FeatureToggles { get; set; } = new List<FeatureToggle>();

        public void AddDeployEnvironment(string name, bool defaultToggleValue, int sortOrder = 1)
        {
            //TODO: do some checks here for parameters
            if (DeploymentEnvironments.Exists(e => e.EnvName == name))
                throw new BusinessRuleValidationException("Environment with the same name already exists for this application!");

            DeploymentEnvironments.Add(DeployEnvironment.Create(name, defaultToggleValue, sortOrder));

            foreach (var ft in FeatureToggles)
            {
                ft.AddStatus(defaultToggleValue, name);
            }
        }

        public static Application Create(string appName, string defaultEnvironmentName, bool defaultToggleValueForEnvironment)
        {
            //TODO: do some checks here for parameters
            var app = new Application
            {
                Id = Guid.NewGuid(),
                AppName = appName
            };
            app.AddDeployEnvironment(defaultEnvironmentName, defaultToggleValueForEnvironment);
            return app;
        }

        public void AddFeatureToggle(string toggleName, string notes, bool isPermanent = false)
        {
            if (FeatureToggles.Exists(f => f.ToggleName == toggleName))
            {
                throw new BusinessRuleValidationException("Feature toggle with the same name already exists for this application!");
            }

            var ft = FeatureToggle.Create(toggleName, notes, isPermanent, DeploymentEnvironments);
            FeatureToggles.Add(ft);
        }

        public FeatureToggleStatusData GetFeatureToggleStatus(string toggleName, string environment)
        {
            var toggle = FeatureToggles.FirstOrDefault(f => f.ToggleName == toggleName);
            return toggle.FeatureToggleStatuses.Where(fts => fts.EnvironmentName == environment).Select(x => new FeatureToggleStatusData
            {
                EnvironmentName = x.EnvironmentName,
                Enabled = x.Enabled
            }).FirstOrDefault();
        }

        public void DeleteDeployEnvironment(string environment)
        {
            if (!DeploymentEnvironments.Exists(e => e.EnvName == environment))
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
            var env = DeploymentEnvironments.FirstOrDefault(e => e.EnvName == oldName);

            if (env == null)
                throw new InvalidOperationException("Environment does not exist!");

            env.EnvName = newName;
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
                UserAccepted = toggle.UserAccepted
            };
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
            if (FeatureToggles.Exists(t => t.ToggleName == newName && t.Id != toggleId))
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
                Enabled = x.Enabled
            }).ToList();
        }

        public void SetToggle(Guid toggleId, string environment, bool isEnabled)
        {
            var toggle = GuardToggleExists(toggleId);
            toggle.Toggle(environment, isEnabled);
        }

        private FeatureToggle GuardToggleExists(Guid toggleId)
        {
            var toggle = FeatureToggles.FirstOrDefault(f => f.Id == toggleId);
            if (toggle is null)
                throw new InvalidOperationException("Feature toggle not found!");
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

        #region DTOs

        public struct ToggleData
        {
            public string ToggleName { get; set; }
            public bool UserAccepted { get; set; }
            public string Notes { get; set; }
            public bool IsPermanent { get; set; }
        }

        public struct FeatureToggleStatusData
        {
            public bool Enabled { get; set; }
            public string EnvironmentName { get; set; }
        }
        #endregion
    }
}