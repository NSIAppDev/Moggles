#if NETCORE
using System;
using Microsoft.Extensions.Configuration;
using MogglesClient.PublicInterface;

namespace MogglesClient
{
    public class NetCoreMogglesConfigurationManager: IMogglesConfigurationManager
    {
        private IConfiguration Configuration { get; set; }

        public NetCoreMogglesConfigurationManager(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string GetApplicationName()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.ApplicationName] ?? throw new MogglesClientException("There is no \"Application\" value defined in the configuration file");
        }

        public string GetEnvironment()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.Environment] ?? throw new MogglesClientException("There is no \"Environment\" value defined in the configuration file");
        }

        public string GetMessageBusUrl()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.MessageBusUrl];
        }

        public string GetMessageBusUser()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.MessageBusUser];
        }

        public string GetMessageBusPassword()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.MessageBusPassword];
        }

        public string GetTogglesUrl()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.Url] ?? throw new MogglesClientException("There is no \"Url\" value defined in the configuration file");
        }

        public TimeSpan GetTimeoutValue()
        {
            var timeoutString =
                Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.RequestTimeout] ?? ConfigurationKeys.DefaultTimeoutValue;
            return TimeSpan.FromSeconds(int.Parse(timeoutString));
        }

        public DateTimeOffset GetCachingTime()
        {
            var cachingTime = Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.CachingTime];

            return cachingTime != null
                ? DateTimeOffset.UtcNow.AddSeconds(Int32.Parse(cachingTime))
                : DateTimeOffset.UtcNow.AddSeconds(ConfigurationKeys.DefaultCachingTime);
        }

        public DateTimeOffset GetOnErrorCachingTime()
        {
            return DateTimeOffset.UtcNow.AddSeconds(ConfigurationKeys.OnErrorCachingTime);
        }

        public bool IsApplicationInTestingMode()
        {
            var isApplicationInTestingMode =
                Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.TestingMode];

            return isApplicationInTestingMode != null && Convert.ToBoolean(isApplicationInTestingMode);
        }

        public bool GetFeatureToggleValueFromConfig(string name)
        {
            return Convert.ToBoolean(
                Configuration.GetSection(ConfigurationKeys.RootSection)[name]);
        }

        public bool IsMessagingEnabled()
        {
            var useMessaging = Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.UseMessaging] ?? ConfigurationKeys.UseMessagingDefault;
            return Convert.ToBoolean(useMessaging);
        }
        public string[] GetCustomAssemblies()
        {
            var customAssembliesString = Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.CustomAssembliesToIgnore];
            return !string.IsNullOrEmpty(customAssembliesString) ? customAssembliesString.Split(',') : new string[] {};
        }

        public string GetInstrumentationKey()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.InstrumentationKey];
        }

        public string GetCacheRefreshQueue()
        {
            return Configuration.GetSection(ConfigurationKeys.RootSection)[ConfigurationKeys.CacheRefreshQueue];
        }
    }
}
#endif