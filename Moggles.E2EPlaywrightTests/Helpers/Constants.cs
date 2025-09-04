using Microsoft.Extensions.Configuration;

namespace Moggles.E2EPlaywrightTests.Helpers
{
    public static class Constants
    {
        public static Random random = new Random();

        public static string BaseUrl { get; set; } = "https://moggles.northernsafety-dev.com/";

        public static string FeatureToggleName = "SmokeTestsFeatureToggle" + random.Next(10);
        public static string SmokeTestsApplication = "SmokeTests";
        public static string AcceptedByUserStatus = "Accepted";
        public static string NewApplicationName = "ApplicationToDelete";
        public static string EditedApplicationName = "EditedApplicationName";
        public static string FirstEnvName = "DEV";
        public static string SecondEnvName = "QA";
        public static string EditedSecondEnvName = "EditedQA";
        public static string DeleteToggleReason = "Test delete toggle";

    }
}
