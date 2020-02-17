using System;

namespace Moggles.Domain
{
    public class DeployEnvironment : Entity
    {
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int SortOrder { get; set; }
        public bool RequireReasonForChangeWhenTrue { get; set; }
        public bool RequireReasonForChangeWhenFalse { get; set; }

        public static DeployEnvironment Create(string name, bool defaultToggleValue, bool requireReasonWhenTrue,bool requireReasonWhenFalse,int sortOrder = 1)
        {
            return new DeployEnvironment
            {
                DefaultToggleValue = defaultToggleValue,
                EnvName = name,
                SortOrder = sortOrder,
                RequireReasonForChangeWhenTrue = requireReasonWhenTrue,
                RequireReasonForChangeWhenFalse =  requireReasonWhenFalse,
                Id = Guid.NewGuid()
            };
        }
    }
}