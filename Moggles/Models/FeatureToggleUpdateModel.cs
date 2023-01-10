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
        public int? Status { get; set; }
        public string HoldReason { get; set; }
        public string Notes { get; set; }
        public bool IsPermanent { get; set; }
        public List<FeatureToggleStatusUpdateModel> Statuses { get; set; }
        public string WorkItemIdentifier { get; set; }
        public ReasontoChangeUpdateModel ReasonToChange { get; set; }
    }

    public class FeatureToggleStatusUpdateModel
    {
        public bool Enabled { get; set; }
        public string Environment { get; set; }
        public string UpdatedByUser { get; set; }
    }

    public class ReasontoChangeUpdateModel
    {
        public string Description { get; set; }
        public List<string> Environments { get; set; }
    }
}
