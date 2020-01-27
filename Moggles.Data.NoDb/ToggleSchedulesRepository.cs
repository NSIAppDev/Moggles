using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moggles.Domain;
using NoDb;

namespace Moggles.Data.NoDb
{
    public class ToggleSchedulesRepository : IRepository<ToggleSchedule>
    {
        private readonly IBasicQueries<ToggleSchedule> _toggleScheduleQueries;
        private readonly IBasicCommands<ToggleSchedule> _toggleScheduleCommands;
        private const string ProjectId = "moggles";
        public ToggleSchedulesRepository(IBasicQueries<ToggleSchedule> toggleScheduleQueries, IBasicCommands<ToggleSchedule> toggleScheduleCommands) 
        {
            _toggleScheduleQueries = toggleScheduleQueries;
            _toggleScheduleCommands = toggleScheduleCommands;
        }

        async Task<IEnumerable<ToggleSchedule>> IRepository<ToggleSchedule>.GetAllAsync()
        {
            return await _toggleScheduleQueries.GetAllAsync(ProjectId).ConfigureAwait(false);
        }

        async Task IRepository<ToggleSchedule>.AddAsync(ToggleSchedule entity)
        {
            await _toggleScheduleCommands.CreateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task IRepository<ToggleSchedule>.DeleteAsync(ToggleSchedule entity)
        {
            await _toggleScheduleCommands.DeleteAsync(ProjectId, entity.Id.ToString());
        }

        async Task IRepository<ToggleSchedule>.UpdateAsync(ToggleSchedule entity)
        {
            await _toggleScheduleCommands.UpdateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task<ToggleSchedule> IRepository<ToggleSchedule>.FindByIdAsync(Guid id)
        {
            return await _toggleScheduleQueries.FetchAsync(ProjectId, id.ToString());
        }
    }
}

