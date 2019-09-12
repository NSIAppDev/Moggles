using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.Domain
{
    public class Application : Entity, IAggregateRoot
    {
        public string AppName { get; set; }

        //private readonly List<DeployEnvironment> _deploymentEnvironments = new List<DeployEnvironment>();
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

        public FeatureToggleStatus GetFeatureToggleStatus(string toggleName, string environment)
        {
            var toggle = FeatureToggles.FirstOrDefault(f => f.ToggleName == toggleName);
            return toggle.FeatureToggleStatuses.FirstOrDefault(fts => fts.EnvironmentName == environment);
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
    }
}