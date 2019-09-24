using Moggles.Domain;
using NoDb;

namespace Moggles.Data.NoDb
{
    public class ToggleSchedulesRepository : BasicNoDbRepository<ToggleSchedule>
    {
        public ToggleSchedulesRepository(IBasicQueries<ToggleSchedule> applicationQueries, IBasicCommands<ToggleSchedule> applicationCommands) : base(applicationQueries, applicationCommands)
        {
        }
    }
}

