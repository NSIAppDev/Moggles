using System;
using System.Collections.Generic;

namespace Moggles.Domain
{
    public class Application : Entity, IAggregateRoot
    {
        public string AppName { get; set; }
        public List<DeployEnvironment> DeploymentEnvironments { get; set; } = new List<DeployEnvironment>();
        public List<FeatureToggle> FeatureToggles { get; set; } = new List<FeatureToggle>();
    }
}