using System;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class UpdateApplicationModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ApplicationName { get; set; }
    }
}
