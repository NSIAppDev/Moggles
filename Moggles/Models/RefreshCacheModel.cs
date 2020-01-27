using System;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class RefreshCacheModel
    {
        public Guid ApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnvName { get; set; }
    }
}
