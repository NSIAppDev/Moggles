namespace Moggles.Domain.DTO
{
    public struct FeatureToggleStatusData
    {
        public bool Enabled { get; set; }
        public string EnvironmentName { get; set; }
    }
}