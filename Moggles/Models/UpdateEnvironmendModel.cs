using System;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class UpdateEnvironmentModel
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required] [MaxLength(50)]
        public string InitialEnvName { get; set; }

        [Required] [MaxLength(50)]
        public string NewEnvName { get; set; }

        public int SortOrder { get; set; }

        public bool DefaultToggleValue { get; set; }

        public bool RequireReasonForChangeWhenToggleEnabled{ get; set; }
        public bool RequireReasonForChangeWhenToggleDisabled { get; set; }

        public bool MoveToLeft { get; set; }
        public bool MoveToRight { get; set; }
    }
}
