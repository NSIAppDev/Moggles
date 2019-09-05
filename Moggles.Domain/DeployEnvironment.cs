using System;

namespace Moggles.Domain
{
    public class DeployEnvironment 
    {
        public Guid Id { get; set; }
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int SortOrder { get; set; }
    }
}