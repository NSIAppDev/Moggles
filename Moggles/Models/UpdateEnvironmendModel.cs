using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class UpdateEnvironmentModel
    {
        public int Id { get; set; }
        [Required] [MaxLength(50)] public string EnvName { get; set; }
    }
}
