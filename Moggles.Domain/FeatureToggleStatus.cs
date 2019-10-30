using System;

namespace Moggles.Domain
{
    public class FeatureToggleStatus : Entity
    {
        public string EnvironmentName { get; set; }
        public bool Enabled { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime? LastDeployStatusUpdate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string UpdatedbyUser { get; set; }

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

        public void ToggleStatus(bool isEnabled, string username)
        {
            UpdateTimestampOnChange(isEnabled, username);
            Enabled = isEnabled;
        }

        private void UpdateTimestampOnChange(bool isEnabled, string username)
        {
            if (Enabled != isEnabled)
            {
                LastUpdated = DateTime.UtcNow;
                ChangeLastUpdateUser(username);
            }
        }

        public void ChangeLastUpdateUser(string username)
        {
            if(username != UpdatedbyUser)
            {
                UpdatedbyUser = username;
            }
        }

        public static FeatureToggleStatus Create(string envName, bool enabled)
        {
            return new FeatureToggleStatus
            {
                Id = Guid.NewGuid(),
                Enabled = enabled,
                EnvironmentName = envName,
                LastUpdated = DateTime.UtcNow,
                UpdatedbyUser = "System"
            };
        }
    }
}