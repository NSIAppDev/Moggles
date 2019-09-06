using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;
using Moggles.Domain;

namespace Moggles.Repository
{
    public class ApplicationsRepository : IRepository<Application>
    {
        private IBasicQueries<Application> _applicationQueries;
        private IBasicCommands<Application> _applicationCommands;
        private const string projectId = "moggles";

        public ApplicationsRepository(IBasicQueries<Application> applicationQueries, IBasicCommands<Application> applicationCommands)
        {
            _applicationQueries = applicationQueries;
            _applicationCommands = applicationCommands;
        }

        async Task<IEnumerable<Application>> IRepository<Application>.GetAll()
        {
            return await _applicationQueries.GetAllAsync(projectId).ConfigureAwait(false);
        }

        void IRepository<Application>.Add(Application entity)
        {
            _applicationCommands.CreateAsync(projectId, entity.Id.ToString(), entity);
        }

        void IRepository<Application>.Delete(Application entity)
        {
            _applicationCommands.DeleteAsync(projectId, entity.Id.ToString());
        }

        void IRepository<Application>.Update(Application entity)
        {
            _applicationCommands.UpdateAsync(projectId, entity.Id.ToString(), entity);
        }

        async Task<Application> IRepository<Application>.FindById(Guid Id)
        {
            return await _applicationQueries.FetchAsync(projectId, Id.ToString());
        }
    }
}

