using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class AddEnvironmentModel
    {
        public int ApplicationId { get; set; }
        [Required]
        [MaxLength(50)]
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int SortOrder { get; set; }
    }
}
