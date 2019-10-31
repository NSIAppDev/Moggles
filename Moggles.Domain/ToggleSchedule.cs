using System;
using System.Collections.Generic;

namespace Moggles.Domain
{
    public class ToggleSchedule : AggregateRoot
    {
        public string ToggleName { get;  set; }
        public string ApplicationName { get;  set; }
        public List<string> Environments { get; set; } = new List<string>();
        public bool ScheduledState { get;  set; }
        public DateTime ScheduledDate { get;  set; }
        public string UpdatedBy { get; set; }

        private ToggleSchedule()
        {
        }

        public static ToggleSchedule Create(string appName, string toggleName, IEnumerable<string> environments, bool stateToSet, DateTime schedule, string createdBy)
        {
            var ts = new ToggleSchedule
            {
                Id = Guid.NewGuid(),
                ApplicationName = appName,
                ToggleName = toggleName,
                ScheduledState = stateToSet,
                ScheduledDate = schedule,
                UpdatedBy =  createdBy
            };
            ts.Environments.AddRange(environments);
            return ts;
        }

        public bool IsDue()
        {
            return ScheduledDate < DateTime.UtcNow;
        }
    }
}