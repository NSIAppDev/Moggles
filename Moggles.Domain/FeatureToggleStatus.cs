﻿using System;

namespace Moggles.Domain
{
    public class FeatureToggleStatus : Entity
    {
        public Guid EnvironmentId { get; set; }
        public string EnvironmentName { get; set; }
        public bool Enabled { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime? LastDeployStatusUpdate { get; set; }
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

        public void ToggleStatus(bool isEnabled)
        {
            UpdateTimestampOnChange(isEnabled);
            Enabled = isEnabled;
        }

        private void UpdateTimestampOnChange(bool isEnabled)
        {
            if (Enabled != isEnabled)
                LastUpdated = DateTime.UtcNow;
        }

        public static FeatureToggleStatus Create(string envName, bool enabled)
        {
            return new FeatureToggleStatus
            {
                Id = Guid.NewGuid(),
                Enabled = enabled,
                EnvironmentName = envName,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}