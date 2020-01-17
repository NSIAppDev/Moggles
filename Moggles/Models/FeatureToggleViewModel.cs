using System;
using System.Collections.Generic;

namespace Moggles.Models
{
    public class FeatureToggleViewModel
    {
        public Guid Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public bool IsPermanent { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<FeatureToggleStatusViewModel> Statuses { get; set; }
        public string WorkItemIdentifier { get; set; }
    }

    public class FeatureToggleStatusViewModel
    {
        public Guid Id { get; set; }
        public bool Enabled { get; set; }
        public string Environment { get; set; }
        public bool IsDeployed { get; set; }
        public DateTime? FirstTimeDeployDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string UpdatedByUser { get; set; }
    }
}
