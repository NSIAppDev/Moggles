using System;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class FeatureToggleStatus : IEntity
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime? LastDeployStatusUpdate { get; set; }
        public Guid EnvironmentId { get; set; }
        public Guid FeatureToggleId { get; set; }
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