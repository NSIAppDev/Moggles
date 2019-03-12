using System;
using System.Collections.Generic;

namespace Moggles.Models
{
    public class FeatureToggleViewModel
    {
        public int Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<FeatureToggleStatusViewModel> Statuses { get; set; }
    }

    public class FeatureToggleStatusViewModel
    {
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Environment { get; set; }
        public bool IsDeployed { get; set; }
    }
}
