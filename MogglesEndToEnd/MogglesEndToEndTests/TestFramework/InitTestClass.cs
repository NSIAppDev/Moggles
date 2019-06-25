using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MogglesEndToEndTests.TestFramework
{
    [TestClass]
    public class InitTestClass
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            if (!context.Properties.Contains("webAppUrl"))
                return;

            var appUrl = context.Properties["webAppUrl"]?.ToString();

            if (!string.IsNullOrEmpty(appUrl))
                Constants.BaseUrl = appUrl;
        }
    }
}
