using System;
using System.Collections.Generic;
using System.Linq;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class FeatureToggle : IEntity
    {
        public Guid Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }
        public Guid ApplicationId { get; set; }
    }
}