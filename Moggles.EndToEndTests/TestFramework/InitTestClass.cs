using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesEndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.TestFramework
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
            var user = context.Properties["user"]?.ToString();
            var password = context.Properties["password"]?.ToString();

            if (!string.IsNullOrEmpty(appUrl) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                Constants.BaseUrl = "https://" + user + ":" + password + "@" + appUrl;
                Constants.MogglesUser = user;
                Constants.MogglesPassword = password;
            }
        }

        [TestMethod]
        public void WramupTest()
        {

        }
    }
}