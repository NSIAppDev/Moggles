using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class UpdateEnvironmentModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required] [MaxLength(50)]
        public string InitialEnvName { get; set; }

        [Required] [MaxLength(50)]
        public string NewEnvName { get; set; }
    }
}
