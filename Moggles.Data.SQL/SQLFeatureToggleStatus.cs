using System;

namespace Moggles.Data.SQL
{
    public class SQLFeatureToggleStatus
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime? LastDeployStatusUpdate { get; set; }

        public SQLDeployEnvironment Environment { get; set; }
       

        public SQLFeatureToggle FeatureToggle { get; set; }
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