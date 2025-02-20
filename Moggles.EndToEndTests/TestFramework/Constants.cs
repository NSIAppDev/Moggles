using System;

namespace Moggles.EndToEndTests.TestFramework
{
    internal class Constants
    {
        public static Random random = new Random();

        public static string MogglesUser;
        public static string MogglesPassword;
        public static string BaseUrl;
        public static string EnvUrl;

        public static string FeatureToggleName = "SmokeTestsFeatureToggle"+random.Next(10);
        public static string SmokeTestsApplication = "SmokeTests";
        public static string AcceptedByUserStatus = "Accepted";
        public static string NewApplicationName = "ApplicationToDelete";
        public static string EditedApplicationName = "EditedApplication";
        public static string FirstEnvName = "DEV";
        public static string SecondEnvName = "QA";
        public static string EditedSecondEnvName = "EditedQA";
        public static string DeleteToggleReason = "Test delete toggle";
    }
}
