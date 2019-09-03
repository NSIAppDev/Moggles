using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;

namespace Moggles.Domain.Repository
{
    public class ApplicationsRepository : IRepository<Application>
    {
        private readonly IBasicQueries<Application> _applicationQueries;
        private readonly IBasicCommands<Application> _applicationCommands;

        public ApplicationsRepository(IBasicQueries<Application> applicationQueries, IBasicCommands<Application> applicationCommands)
        {
            _applicationQueries = applicationQueries;
            _applicationCommands = applicationCommands;
        }

        public async Task<IEnumerable<Application>> GetAll()
        {
            return await _applicationQueries.GetAllAsync(Constants.ProjectId).ConfigureAwait(false);
        }

        public void Add(Application entity)
        {
            _applicationCommands.CreateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public void Delete(Application entity)
        {
            _applicationCommands.DeleteAsync(Constants.ProjectId, entity.Id.ToString());
        }

        public void Update(Application entity)
        {
            _applicationCommands.UpdateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public async Task<Application> FindById(Guid id)
        {
            return await _applicationQueries.FetchAsync(Constants.ProjectId, id.ToString());
        }
    }
}

