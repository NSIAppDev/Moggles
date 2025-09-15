using System.Text.Json.Serialization;

namespace Moggles.E2EPlaywrightTests.Helpers.Models
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }

    public class ApplicationDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("appName")]
        public string AppName { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonPropertyName("hasBeenMigrated")]
        public bool HasBeenMigrated { get; set; }

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }
    }


}
