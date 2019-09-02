using System;
using Moggles.Repository;

namespace Moggles.Domain
{
    public class DeployEnvironment : IEntity
    {
        public Guid Id { get; set; }
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }
        public int SortOrder { get; set; }
    }
}