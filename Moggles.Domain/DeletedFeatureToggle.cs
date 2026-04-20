using System;
using System.Collections.Generic;

namespace Moggles.Domain
{
    public class DeletedFeatureToggle : Entity
    {
        public string ToggleName { get; set; }
        public string Reason { get; set; }
        public DateTime DeletionDate { get; set; }
        public List<DeletedFeatureToggleStatus> StatusesAtDeletion { get; set; } = new List<DeletedFeatureToggleStatus>();
    }

    public class DeletedFeatureToggleStatus
    {
        public string EnvironmentName { get; set; }
        public bool Enabled { get; set; }
    }
}
