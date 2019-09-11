using System;
using System.Collections.Generic;


namespace Moggles.Domain
{
    public class FeatureToggle: Entity
    {
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }
        public List<FeatureToggleStatus> FeatureToggleStatuses { get; set; }
    }
}