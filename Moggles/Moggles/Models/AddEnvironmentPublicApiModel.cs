using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class AddEnvironmentPublicApiModel
    {
        [Required]
        [MaxLength(100)]
        public string AppName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string EnvName { get; set; }
    }
}
