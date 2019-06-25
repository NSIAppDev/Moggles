namespace MogglesEndToEndTests.TestFramework
{
    public class Constants
    {
        public static string BaseUrl = "http://rtoadere:RAMSafety4@moggles.northernsafety-dev.com/";
        public static string FeatureToggleName = "SmokeTestsFeatureToggle";
        public static string SmokeTestsApplication = "SmokeTests";
        public static string AcceptedByUserStatus = "Accepted";
        public static string UnacceptedStatus = "Unaccepted";

        public static bool HeadlessModeEnabled { get; set; }
        public static int WaitTime { get; } = 60;
    }
}
