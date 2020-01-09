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
        public bool ForceCacheRefresh { get; set; }

        private ToggleSchedule()
        {
        }

        public static ToggleSchedule Create(string appName, string toggleName, IEnumerable<string> environments, bool stateToSet, DateTime schedule, string createdBy, bool refreshCache=false)
        {
            var ts = new ToggleSchedule
            {
                Id = Guid.NewGuid(),
                ApplicationName = appName,
                ToggleName = toggleName,
                ScheduledState = stateToSet,
                ScheduledDate = schedule,
                UpdatedBy =  createdBy,
                ForceCacheRefresh=refreshCache
            };
            ts.Environments.AddRange(environments);
            return ts;
        }

        public bool IsDue()
        {
            return ScheduledDate.ToUniversalTime() < DateTime.UtcNow;
        }   

        public bool EnvExists(string name)
        {
            return Environments.Exists(envs => string.Compare(envs, name, StringComparison.OrdinalIgnoreCase) ==0);
        }

        public void AddEnvironment(string env)
        {
           Environments.Add(env);
        }

        public void RemoveEnvironment(string name)
        {
            Environments.RemoveAll(env => string.Compare(env, name) == 0);
        }
        public void ChangeState(bool state)
        {
            ScheduledState = state;
        }

        public void ChangeDate(DateTime scheduledDateTime)
        {
                ScheduledDate = scheduledDateTime;
        }
        public void ChangeUpdatedBy(string updatedBy)
        {
            UpdatedBy = updatedBy;
        }
        public void ChangeForceCacheRefresh(bool refreshCache)
        {
            ForceCacheRefresh = refreshCache;
        }
    }
}