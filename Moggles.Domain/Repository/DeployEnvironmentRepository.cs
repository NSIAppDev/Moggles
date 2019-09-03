using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;

namespace Moggles.Domain.Repository
{
    public class DeployEnvironmentRepository : IRepository<DeployEnvironment>
    {
        private readonly IBasicQueries<DeployEnvironment> _deployEnvironmentQueries;
        private readonly IBasicCommands<DeployEnvironment> _deployEnvironmentCommands;

        public DeployEnvironmentRepository(IBasicQueries<DeployEnvironment> deployEnvironmentQueries, IBasicCommands<DeployEnvironment> deployEnvironmentCommands)
        {
            _deployEnvironmentQueries = deployEnvironmentQueries;
            _deployEnvironmentCommands = deployEnvironmentCommands;
        }

        public async Task<IEnumerable<DeployEnvironment>> GetAll()
        {
            return await _deployEnvironmentQueries.GetAllAsync(Constants.ProjectId).ConfigureAwait(false);
        }

        public void Add(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.CreateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public void Delete(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.DeleteAsync(Constants.ProjectId, entity.Id.ToString());
        }

        public void Update(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.UpdateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public async Task<DeployEnvironment> FindById(Guid id)
        {
            return await _deployEnvironmentQueries.FetchAsync(Constants.ProjectId, id.ToString());
        }
    }
}

