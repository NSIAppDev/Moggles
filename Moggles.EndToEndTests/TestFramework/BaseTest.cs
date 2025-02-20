using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace Moggles.EndToEndTests.TestFramework
{
    public class BaseTest : NsTestFrameworkUI.BaseTest
    {
        [TestInitialize]
        public virtual void Before()
        {
            KeyVaultHelper.InitializeVault("AppDev-Dev");
            NsTestFrameworkUI.Helpers.Browser.InitializeDriver();
        }

        [TestCleanup]
        public virtual void After()
        {
            NsTestFrameworkUI.Helpers.Browser.Cleanup();
            NsTestFrameworkUI.Helpers.Browser.WebDriver.Dispose();
            NsTestFrameworkUI.Helpers.Browser.WebDriver.Quit();
        }
    }
}
