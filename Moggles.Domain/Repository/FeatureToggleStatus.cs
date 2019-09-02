using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;
using Moggles.Domain;

namespace Moggles.Repository
{
    public class FeatureToggleStatusRepository : IRepository<FeatureToggleStatus>
    {
        private IBasicQueries<FeatureToggleStatus> _featureToggleStatusQueries;
        private IBasicCommands<FeatureToggleStatus> _featureToggleStatusCommands;
        private const string projectId = "moggles";

        public FeatureToggleStatusRepository(IBasicQueries<FeatureToggleStatus> FeatureToggleStatusQueries, IBasicCommands<FeatureToggleStatus> FeatureToggleStatusCommands)
        {
            _featureToggleStatusQueries = FeatureToggleStatusQueries;
            _featureToggleStatusCommands = FeatureToggleStatusCommands;
        }

        async Task<IEnumerable<FeatureToggleStatus>> IRepository<FeatureToggleStatus>.GetAll()
        {
            return await _featureToggleStatusQueries.GetAllAsync(projectId).ConfigureAwait(false);
        }

        void IRepository<FeatureToggleStatus>.Add(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.CreateAsync(projectId, entity.Id.ToString(), entity);
        }

        void IRepository<FeatureToggleStatus>.Delete(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.DeleteAsync(projectId, entity.Id.ToString());
        }

        void IRepository<FeatureToggleStatus>.Update(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.UpdateAsync(projectId, entity.Id.ToString(), entity);
        }

        async Task<FeatureToggleStatus> IRepository<FeatureToggleStatus>.FindById(Guid Id)
        {
            return await _featureToggleStatusQueries.FetchAsync(projectId, Id.ToString());
        }
    }
}