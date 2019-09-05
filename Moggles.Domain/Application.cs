using System;
using System.Collections.Generic;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class Application : IEntity
    {
        public Guid Id { get; set; }
        public string AppName { get; set; }
        public List<DeployEnvironment> DeploymentEnvironments { get; set; } = new List<DeployEnvironment>();
        public List<FeatureToggle> FeatureToggles { get; set; } = new List<FeatureToggle>();
    }
}