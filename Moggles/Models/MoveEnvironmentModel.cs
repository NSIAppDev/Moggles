using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Moggles.Models
{
    public class MoveEnvironmentModel
    {
        [Required]
        public Guid ApplicationId { get; set; }
        public string EnvName { get; set; }
        public bool MoveToLeft { get; set; }
        public bool MoveToRight { get; set; }
    }
}
