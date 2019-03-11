namespace MogglesClient.Messaging.EnvironmentDetector
{
    public interface IFeatureToggleEnvironmentDetector
    {
        void RegisterDeployedToggles();
    }
}
