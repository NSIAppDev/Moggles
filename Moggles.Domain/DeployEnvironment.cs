namespace Moggles.Domain
{
    public class DeployEnvironment
    {
        public int Id { get; set; }
        public string EnvName { get; set; }
        public bool DefaultToggleValue { get; set; }
        public int ApplicationId { get; set; }
        public Application Application { get; set; }
        public int SortOrder { get; set; }
    }
}