using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;

namespace Moggles.Domain.Repository
{
    public class FeatureToggleRepository : IRepository<FeatureToggle>
    {
        private readonly IBasicQueries<FeatureToggle> _featureToggleQueries;
        private readonly IBasicCommands<FeatureToggle> _featureToggleCommands;

        public FeatureToggleRepository(IBasicQueries<FeatureToggle> featureToggleQueries, IBasicCommands<FeatureToggle> featureToggleCommands)
        {
            _featureToggleQueries = featureToggleQueries;
            _featureToggleCommands = featureToggleCommands;
        }

        public async Task<IEnumerable<FeatureToggle>> GetAll()
        {
            return await _featureToggleQueries.GetAllAsync(Constants.ProjectId).ConfigureAwait(false);
        }

        public void Add(FeatureToggle entity)
        {
            _featureToggleCommands.CreateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public void Delete(FeatureToggle entity)
        {
            _featureToggleCommands.DeleteAsync(Constants.ProjectId, entity.Id.ToString());
        }

        public void Update(FeatureToggle entity)
        {
            _featureToggleCommands.UpdateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public async Task<FeatureToggle> FindById(Guid id)
        {
            return await _featureToggleQueries.FetchAsync(Constants.ProjectId, id.ToString());
        }
    }
}