using System;

namespace MogglesClient.PublicInterface
{
    public interface IMogglesConfigurationManager
    {
        string GetApplicationName();
        string GetEnvironment();
        string GetMessageBusUrl();
        string GetMessageBusUser();
        string GetMessageBusPassword();
        string GetTogglesUrl();
        TimeSpan GetTimeoutValue();
        DateTimeOffset GetCachingTime();
        DateTimeOffset GetOnErrorCachingTime();
        bool IsApplicationInTestingMode();
        bool GetFeatureToggleValueFromConfig(string name);
        bool IsMessagingEnabled();
        string[] GetCustomAssemblies();
        string GetInstrumentationKey();
        string GetCacheRefreshQueue();
    }
}
