using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class AddFeatureToggleModel
    {
        public int ApplicationId { get; set; }

        [Required]
        [MaxLength(80)]
        public string FeatureToggleName { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }
    }
}
