using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moggles.Domain;
using NoDb;

namespace Moggles.Data.NoDb
{
    public class BasicNoDbRepository<T> : IRepository<T> where T: AggregateRoot
    {
        private readonly IBasicQueries<T> _applicationQueries;
        private readonly IBasicCommands<T> _applicationCommands;
        private const string ProjectId = "moggles";

        public BasicNoDbRepository(IBasicQueries<T> applicationQueries, IBasicCommands<T> applicationCommands)
        {
            _applicationQueries = applicationQueries;
            _applicationCommands = applicationCommands;
        }

        async Task<IEnumerable<T>> IRepository<T>.GetAllAsync()
        {
            return await _applicationQueries.GetAllAsync(ProjectId).ConfigureAwait(false);
        }

        async Task IRepository<T>.AddAsync(T entity)
        {
            await _applicationCommands.CreateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task IRepository<T>.DeleteAsync(T entity)
        {
            await _applicationCommands.DeleteAsync(ProjectId, entity.Id.ToString());
        }

        async Task IRepository<T>.UpdateAsync(T entity)
        {
            await _applicationCommands.UpdateAsync(ProjectId, entity.Id.ToString(), entity);
        }

        async Task<T> IRepository<T>.FindByIdAsync(Guid Id)
        {
            return await _applicationQueries.FetchAsync(ProjectId, Id.ToString());
        }
    }
}

