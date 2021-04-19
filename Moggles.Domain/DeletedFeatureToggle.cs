namespace Moggles.Domain
{
    public class DeletedFeatureToggle : Entity
    {
        public string ToggleName { get; set; }
        public string Reason { get; set; }
    }
}
