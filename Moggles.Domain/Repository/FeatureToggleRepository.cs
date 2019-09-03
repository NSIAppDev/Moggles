using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;
using Moggles.Domain;
using Moggles.Domain.Repository;

namespace Moggles.Repository
{
    public class FeatureToggleRepository : IRepository<FeatureToggle>
    {
        private IBasicQueries<FeatureToggle> _featureToggleQueries;
        private IBasicCommands<FeatureToggle> _featureToggleCommands;
        private const string projectId = "moggles";

        public FeatureToggleRepository(IBasicQueries<FeatureToggle> FeatureToggleQueries, IBasicCommands<FeatureToggle> FeatureToggleCommands)
        {
            _featureToggleQueries = FeatureToggleQueries;
            _featureToggleCommands = FeatureToggleCommands;
        }

        async Task<IEnumerable<FeatureToggle>> IRepository<FeatureToggle>.GetAll()
        {
            return await _featureToggleQueries.GetAllAsync(projectId).ConfigureAwait(false);
        }

        void IRepository<FeatureToggle>.Add(FeatureToggle entity)
        {
            _featureToggleCommands.CreateAsync(projectId, entity.Id.ToString(), entity);
        }

        void IRepository<FeatureToggle>.Delete(FeatureToggle entity)
        {
            _featureToggleCommands.DeleteAsync(projectId, entity.Id.ToString());
        }

        void IRepository<FeatureToggle>.Update(FeatureToggle entity)
        {
            _featureToggleCommands.UpdateAsync(projectId, entity.Id.ToString(), entity);
        }

        async Task<FeatureToggle> IRepository<FeatureToggle>.FindById(Guid Id)
        {
            return await _featureToggleQueries.FetchAsync(projectId, Id.ToString());
        }
    }
}