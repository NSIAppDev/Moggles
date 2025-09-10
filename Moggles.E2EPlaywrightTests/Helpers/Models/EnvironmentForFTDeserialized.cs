using System.Text.Json.Serialization;

namespace Moggles.E2EPlaywrightTests.Helpers.Models
{
    public class EnvironmentForFTDeserialized
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("envName")]
        public string EnvName { get; set; }

        [JsonPropertyName("defaultToggleValue")]
        public bool DefaultToggleValue { get; set; }

        [JsonPropertyName("requireReasonWhenToggleDisabled")]
        public bool RequireReasonWhenToggleEnabled { get; set; }

        [JsonPropertyName("requireReasonWhenToggleEnabled")]
        public bool RequireReasonWhenToggleDisabled { get; set; }

    }
}