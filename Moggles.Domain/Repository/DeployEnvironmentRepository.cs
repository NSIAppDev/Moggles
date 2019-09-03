using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;
using Moggles.Domain;
using Moggles.Domain.Repository;

namespace Moggles.Repository
{
    public class DeployEnvironmentRepository : IRepository<DeployEnvironment>
    {
        private IBasicQueries<DeployEnvironment> _deployEnvironmentQueries;
        private IBasicCommands<DeployEnvironment> _deployEnvironmentCommands;
        private const string projectId = "moggles";

        public DeployEnvironmentRepository(IBasicQueries<DeployEnvironment> deployEnvironmentQueries, IBasicCommands<DeployEnvironment> deployEnvironmentCommands)
        {
            _deployEnvironmentQueries = deployEnvironmentQueries;
            _deployEnvironmentCommands = deployEnvironmentCommands;
        }

        async Task<IEnumerable<DeployEnvironment>> IRepository<DeployEnvironment>.GetAll()
        {
            return await _deployEnvironmentQueries.GetAllAsync(projectId).ConfigureAwait(false);
        }

        void IRepository<DeployEnvironment>.Add(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.CreateAsync(projectId, entity.Id.ToString(), entity);
        }

        void IRepository<DeployEnvironment>.Delete(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.DeleteAsync(projectId, entity.Id.ToString());
        }

        void IRepository<DeployEnvironment>.Update(DeployEnvironment entity)
        {
            _deployEnvironmentCommands.UpdateAsync(projectId, entity.Id.ToString(), entity);
        }

        async Task<DeployEnvironment> IRepository<DeployEnvironment>.FindById(Guid Id)
        {
            return await _deployEnvironmentQueries.FetchAsync(projectId, Id.ToString());
        }
    }
}

