using System.Collections.Generic;

namespace Moggles.Domain.DTO
{
    public struct ToggleData
    {
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public bool IsPermanent { get; set; }
        public string WorkItemIdentifier { get; set; }
        public List<ReasonToChange> ReasonsToChanges { get; set; }
    }
}