using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoDb;

namespace Moggles.Domain.Repository
{
    public class FeatureToggleStatusRepository : IRepository<FeatureToggleStatus>
    {
        private readonly IBasicQueries<FeatureToggleStatus> _featureToggleStatusQueries;
        private readonly IBasicCommands<FeatureToggleStatus> _featureToggleStatusCommands;

        public FeatureToggleStatusRepository(IBasicQueries<FeatureToggleStatus> featureToggleStatusQueries, IBasicCommands<FeatureToggleStatus> featureToggleStatusCommands)
        {
            _featureToggleStatusQueries = featureToggleStatusQueries;
            _featureToggleStatusCommands = featureToggleStatusCommands;
        }

        public async Task<IEnumerable<FeatureToggleStatus>> GetAll()
        {
            return await _featureToggleStatusQueries.GetAllAsync(Constants.ProjectId).ConfigureAwait(false);
        }

        public void Add(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.CreateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public void Delete(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.DeleteAsync(Constants.ProjectId, entity.Id.ToString());
        }

        public void Update(FeatureToggleStatus entity)
        {
            _featureToggleStatusCommands.UpdateAsync(Constants.ProjectId, entity.Id.ToString(), entity);
        }

        public async Task<FeatureToggleStatus> FindById(Guid id)
        {
            return await _featureToggleStatusQueries.FetchAsync(Constants.ProjectId, id.ToString());
        }
    }
}