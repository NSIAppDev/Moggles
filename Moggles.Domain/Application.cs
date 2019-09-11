using System;
using System.Collections.Generic;

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
            DeploymentEnvironments.Add(DeployEnvironment.Create(name, defaultToggleValue, sortOrder));
        }

        public static Application Create(string appName, string defaultEnvironmentName, bool defaultToggleValue)
        {
            //TODO: do some checks here for parameters

            var app = new Application
            {
                Id = Guid.NewGuid(),
                AppName = appName,
            };
            app.AddDeployEnvironment(defaultEnvironmentName,defaultToggleValue);
            return app;
        }

        public void AddFeatureToggle(string toggleName, string notes, bool isPermanent=false)
        {
            //TODO: add code here to check that two toggles cant be added at the same time
            FeatureToggles.Add(new FeatureToggle
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                IsPermanent = isPermanent,
                Notes = notes,
                ToggleName = toggleName
            });
        }
    }
}