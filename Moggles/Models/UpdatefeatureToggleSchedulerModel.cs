using System;
using System.Collections.Generic;

namespace Moggles.Models
{
    public class UpdateFeatureToggleSchedulerModel
    {
        public string ToggleName { get; set; }

        public Guid Id { get; set; }

        public bool ScheduledState { get; set; }

        public DateTime ScheduledDate { get; set; }
        public List<string> Environments { get; set; }
    }
}
