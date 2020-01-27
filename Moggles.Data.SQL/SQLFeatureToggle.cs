using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggles.Data.SQL
{
    public class SQLFeatureToggle
    {
        public SQLFeatureToggle()
        {
            FeatureToggleStatuses = new List<SQLFeatureToggleStatus>();
        }

        public int Id { get; set; }
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPermanent { get; set; }

        public SQLApplication Application { get; set; }
        public int ApplicationId { get; set; }
        public Guid NewId { get; set; } = Guid.NewGuid();

        public List<SQLFeatureToggleStatus> FeatureToggleStatuses { get; set; }

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