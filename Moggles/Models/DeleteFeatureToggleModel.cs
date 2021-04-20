using System;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class DeleteFeatureToggleModel
    {
        public Guid FeatureToggleId { get; set; }
        public Guid ApplicationId { get; set; }
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}
