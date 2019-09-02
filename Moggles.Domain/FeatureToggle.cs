using System;
using System.Collections.Generic;
using System.Linq;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class FeatureToggle : IEntity
    {
        public FeatureToggle()
        {
            FeatureToggleStatuses = new List<FeatureToggleStatus>();
        }

        public Guid Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }

        public Application Application { get; set; }
        public Guid ApplicationId { get; set; }

        public List<FeatureToggleStatus> FeatureToggleStatuses { get; set; }

        public void MarkAsDeployed(string deployEnvironment)
        {
            var environmentStatus = FeatureToggleStatuses.FirstOrDefault(fts => fts.Environment.EnvName.ToUpper() == deployEnvironment.ToUpper());
            environmentStatus?.MarkAsDeployed();
        }

        public void MarkAsNotDeployed(string deployEnvironment)
        {
            var environmentStatus = FeatureToggleStatuses.FirstOrDefault(fts => fts.Environment.EnvName.ToUpper() == deployEnvironment.ToUpper());
            environmentStatus?.MarkAsNotDeployed();
        }
    }
}