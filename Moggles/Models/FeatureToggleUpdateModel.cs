using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class FeatureToggleUpdateModel
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        [Required]
        public string FeatureToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public bool IsPermanent { get; set; }
        public List<FeatureToggleStatusUpdateModel> Statuses { get; set; }
        public string WorkItemIdentifier { get; set; }
        public List<FeatureToggleReasonToChangeModel> ReasonsToChange { get; set; }
    }

    public class FeatureToggleStatusUpdateModel
    {
        public bool Enabled { get; set; }
        public string Environment { get; set; }
        public string UpdatedByUser { get; set; }
    }

    public class FeatureToggleReasonToChangeModel
    {
        public string Description { get; set; }
    }
}
