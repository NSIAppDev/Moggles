using System;

namespace Moggles.Data.SQL
{
    public class SQLDeployEnvironment
    {
        public int Id { get; set; }
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int ApplicationId { get; set; }
        public SQLApplication Application { get; set; }
        public int SortOrder { get; set; }
        public Guid NewId { get; set; } = Guid.NewGuid();
    }
}