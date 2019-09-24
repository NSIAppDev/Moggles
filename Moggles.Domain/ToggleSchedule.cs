using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.Domain
{
    public class ToggleSchedule : AggregateRoot
    {
        public string ToggleName { get; private set; }
        public string ApplicationName { get; private set; }

        private readonly List<string> _environments = new List<string>();
        public IReadOnlyCollection<string> Environments => _environments.ToList();
        public bool ScheduledState { get; private set; }
        public DateTime ScheduledDate { get; private set; }

        private ToggleSchedule()
        {

        }

        public static ToggleSchedule Create(string appName, string toggleName, IEnumerable<string> environments, bool stateToSet, DateTime schedule)
        {
            var ts = new ToggleSchedule
            {
                ApplicationName = appName,
                ToggleName = toggleName,
                ScheduledState = stateToSet,
                ScheduledDate = schedule
            };
            ts._environments.AddRange(environments);
            return ts;
        }

        public bool IsDue()
        {
            return ScheduledDate < DateTime.Now;
        }
    }
}