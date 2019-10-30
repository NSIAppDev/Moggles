using Moggles.EndToEndTests.TestFramework;

namespace MogglesEndToEndTests.TestFramework
{
    public class Constants
    {
        public static string BaseUrl = AppSettingsReader.GetApplicationConfiguration().BaseUrl;

        public static string FeatureToggleName = "SmokeTestsFeatureToggle";
        public static string SmokeTestsApplication = "SmokeTests";
        public static string AcceptedByUserStatus = "Accepted";
        public static string NewApplicationName = "ApplicationToDelete";
        public static string EditedApplicationName = "EditedApplication";
        public static string FirstEnvName = "DEV";
        public static string SecondEnvName = "QA";
        public static string EditedSecondEnvName = "EditedQA";
    }
}
