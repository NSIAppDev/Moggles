using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class ScheduleTogglesModel
    {
        public Guid ApplicationId { get; set; }

        [Required]
        public List<string> FeatureToggles { get; set; }
        [Required]
        public List<string> Environments { get; set; }
        [Required]
        public bool State { get; set; }
        [Required]
        public DateTime ScheduleDate { get; set; }
    }
}