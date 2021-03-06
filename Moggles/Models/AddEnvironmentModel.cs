﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Moggles.Models
{
    public class AddEnvironmentModel
    {
        public Guid ApplicationId { get; set; }
        [Required]
        [MaxLength(50)]
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int SortOrder { get; set; }
        public bool RequireReasonToChangeWhenToggleEnabled { get; set; }
        public bool RequireReasonToChangeWhenToggleDisabled { get; set; }
    }
}
