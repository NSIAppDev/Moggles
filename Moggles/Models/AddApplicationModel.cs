using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class AddApplicationModel
    {
        [Required]
        [MaxLength(100)]
        public string ApplicationName { get; set; }
        [Required]
        [MaxLength(100)]
        public string EnvironmentName { get; set; }
        [Required]
        public bool DefaultToggleValue { get; set; }
    }
}
