using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.Domain
{
    public class FeatureToggle
    {
        public FeatureToggle()
        {
            FeatureToggleStatuses = new List<FeatureToggleStatus>();
        }

        public int Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }

        public Application Application { get; set; }
        public int ApplicationId { get; set; }

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