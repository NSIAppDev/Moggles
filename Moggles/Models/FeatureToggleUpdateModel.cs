using System.Collections.Generic;

namespace Moggles.Models
{
    public class FeatureToggleUpdateModel
    {
        public int Id { get; set; }
        public string FeatureToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public List<FeatureToggleStatusUpdateModel> Statuses { get; set; }
    }

    public class FeatureToggleStatusUpdateModel
    {
        public bool Enabled { get; set; }
        public string Environment { get; set; }
    }
}
