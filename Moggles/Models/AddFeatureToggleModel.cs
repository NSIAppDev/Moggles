using System.ComponentModel.DataAnnotations;
using System;

namespace Moggles.Models
{
    public class AddFeatureToggleModel
    {
        public Guid? ApplicationId { get; set; }

        [Required]
        [MaxLength(80)]
        public string FeatureToggleName { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public bool IsPermanent { get; set; }
    }
}
