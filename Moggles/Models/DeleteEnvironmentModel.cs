using System.ComponentModel.DataAnnotations;
using System;
namespace Moggles.Models
{
    public class DeleteEnvironmentModel
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required] [MaxLength(50)]
        public string EnvName { get; set; }
    }
}
