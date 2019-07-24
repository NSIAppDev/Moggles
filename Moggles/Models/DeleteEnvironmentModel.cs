using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class DeleteEnvironmentModel
    {
        [Required]
        public int ApplicationId { get; set; }

        [Required] [MaxLength(50)]
        public string EnvName { get; set; }
    }
}
