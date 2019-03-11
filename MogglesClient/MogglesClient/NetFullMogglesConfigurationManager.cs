#if NETFULL
using MogglesClient.PublicInterface;
using System;
using System.Configuration;

namespace MogglesClient
{
    public class NetFullMogglesConfigurationManager: IMogglesConfigurationManager
    {
        public string GetApplicationName()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.ApplicationName]
                   ?? throw new MogglesClientException(
                       "There is no \"Application\" value defined in the configuration file");
        }

        public string GetEnvironment()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.Environment]
                   ?? throw new MogglesClientException(
                       "There is no \"Environment\" value defined in the configuration file");
        }

        public string GetMessageBusUrl()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.MessageBusUrl];
        }

        public string GetMessageBusUser()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.MessageBusUser];
        }

        public string GetMessageBusPassword()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.MessageBusPassword];
        }

        public string GetTogglesUrl()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.Url] ?? throw new MogglesClientException("There is no \"Url\" value defined in the configuration file");
        }

        public TimeSpan GetTimeoutValue()
        {
            var timeoutString = ConfigurationManager.AppSettings[ConfigurationKeys.RequestTimeout] ??
                                ConfigurationKeys.DefaultTimeoutValue;

            return TimeSpan.FromSeconds(int.Parse(timeoutString));
        }

        public DateTimeOffset GetCachingTime()
        {
            var cachingTime = ConfigurationManager.AppSettings[ConfigurationKeys.CachingTime];
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
            var isApplicationInTestingMode = ConfigurationManager.AppSettings[ConfigurationKeys.TestingMode];

            return isApplicationInTestingMode != null && Convert.ToBoolean(isApplicationInTestingMode);
        }

        public bool GetFeatureToggleValueFromConfig(string name)
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings[$"{ConfigurationKeys.RootSection}.{name}"]);
        }

        public bool IsMessagingEnabled()
        {
            var useMessaging = ConfigurationManager.AppSettings[ConfigurationKeys.UseMessaging] ?? ConfigurationKeys.UseMessagingDefault;
            return Convert.ToBoolean(useMessaging);
        }

        public string[] GetCustomAssemblies()
        {
            var customAssembliesString = ConfigurationManager.AppSettings[ConfigurationKeys.CustomAssembliesToIgnore];
            return !string.IsNullOrEmpty(customAssembliesString) ? customAssembliesString.Split(',') : new string[]{};
        }

        public string GetInstrumentationKey()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.InstrumentationKey];
        }

        public string GetCacheRefreshQueue()
        {
            return ConfigurationManager.AppSettings[ConfigurationKeys.CacheRefreshQueue];
        }
    }
}
#endif