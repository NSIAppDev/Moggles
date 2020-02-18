using System;

namespace Moggles.Domain
{
    public class DeployEnvironment : Entity
    {
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int SortOrder { get; set; }
        public bool RequireReasonWhenToggleEnabled { get; set; }
        public bool RequireReasonWhenToggleDisabled { get; set; }

        public static DeployEnvironment Create(string name, bool defaultToggleValue, bool requireReasonWhenToggleEnabled,bool requireReasonWhenToggleDisabled,int sortOrder = 1)
        {
            return new DeployEnvironment
            {
                DefaultToggleValue = defaultToggleValue,
                EnvName = name,
                SortOrder = sortOrder,
                RequireReasonWhenToggleEnabled = requireReasonWhenToggleEnabled,
                RequireReasonWhenToggleDisabled =  requireReasonWhenToggleDisabled,
                Id = Guid.NewGuid()
            };
        }
    }
}