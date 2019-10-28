using Microsoft.Extensions.Configuration;
using Moggles.EndToEndTests.TestFramework.Models;
using System;

namespace Moggles.EndToEndTests.TestFramework
{
    internal class AppSettingsReader
    {
        private static string Environment
        {
            get
            {
                var environment = System.Environment.GetEnvironmentVariable("Environment");
                if (string.IsNullOrEmpty(environment))
                {
                    environment = "DEV";
                }
                return environment;
            }
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            var currentDirectory = AppContext.BaseDirectory;

            return new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("testappsettings.json", optional: true)
                .AddJsonFile($"testappsettings.{Environment}.json", true)
                .Build();
        }

        public static AppSettings GetAppSettings()
        {
            var configuration = new AppSettings();

            var iConfig = GetIConfigurationRoot();

            iConfig.GetSection("AppSettings")
                .Bind(configuration);

            return configuration;
        }
    }
}
