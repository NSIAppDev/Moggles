using System;

namespace Moggles.Domain
{
    public class DeletedFeatureToggle : Entity
    {
        public string ToggleName { get; set; }
        public string Reason { get; set; }
        public DateTime DeletionDate { get; } = DateTime.UtcNow;
    }
}
