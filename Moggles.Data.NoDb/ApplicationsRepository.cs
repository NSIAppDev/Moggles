using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moggles.Domain;
using NoDb;

namespace Moggles.Data.NoDb
{
    public class ApplicationsRepository : IRepository<Application>
    {
        private readonly IBasicQueries<Application> _applicationQueries;
        private readonly IBasicCommands<Application> _applicationCommands;
        private const string ProjectId = "moggles";

        public ApplicationsRepository(IBasicQueries<Application> applicationQueries, IBasicCommands<Application> applicationCommands)
        {
            _applicationQueries = applicationQueries;
            _applicationCommands = applicationCommands;
        }

        async Task<IEnumerable<Application>> IRepository<Application>.GetAllAsync()
        {
            return await _applicationQueries.GetAllAsync(ProjectId).ConfigureAwait(false);
        }

        async Task IRepository<Application>.AddAsync(Application entity)
        {
            await _applicationCommands.CreateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task IRepository<Application>.DeleteAsync(Application entity)
        {
            await _applicationCommands.DeleteAsync(ProjectId, entity.Id.ToString());
        }

        async Task IRepository<Application>.UpdateAsync(Application entity)
        {
            await _applicationCommands.UpdateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task<Application> IRepository<Application>.FindByIdAsync(Guid Id)
        {
            return await _applicationQueries.FetchAsync(ProjectId, Id.ToString());
        }
    }
}

