using System;

namespace Moggles.Domain
{
    public class FeatureToggleStatus
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime? LastDeployStatusUpdate { get; set; }

        public DeployEnvironment Environment { get; set; }
        public int EnvironmentId { get; set; }

        public FeatureToggle FeatureToggle { get; set; }
        public int FeatureToggleId { get; set; }

        public DateTime LastUpdated { get; set; }

        public void MarkAsDeployed()
        {
            IsDeployed = true;
            if (!FirstTimeDeployDate.HasValue)
            {
                FirstTimeDeployDate = DateTime.UtcNow;
            }

            LastDeployStatusUpdate = DateTime.UtcNow;
        }

        public void MarkAsNotDeployed()
        {
            IsDeployed = false;
            LastDeployStatusUpdate = DateTime.UtcNow;
        }
    }
}