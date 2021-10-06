using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class DeleteTogglesFromHistoryModel
    {
        [Required]
        public Guid ApplicationId { get; set; }
        [Required]
        public List<Guid> ToggleIds { get; set; }
        
    }
}
