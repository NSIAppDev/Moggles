namespace NSTogglesClient
{
    public class RegisteredTogglesUpdate
    {
        public string Environment { get; set; }
        public string AppName { get; set; }
        public string[] FeatureToggles { get; set; }
    }
}
