using System;

namespace Moggles.EndToEndTests.Helpers
{
    public class EnvironmentForFTDeserialized
    {
        public Guid Id { get; set; }
        public string EnvName { get; set; }
        public string DefaultToggleValue { get; set; }
        public string RequireReasonWhenToggleEnabled { get; set; }
        public string RequireReasonWhenToggleDisabled { get; set; }

    }
}