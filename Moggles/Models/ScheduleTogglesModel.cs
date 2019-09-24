using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class ScheduleTogglesModel
    {
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