using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSTestFrameworkDotNetCoreUI.Helpers;
using Browser = NsTestFrameworkUI.Helpers.Browser;

namespace Moggles.EndToEndTests.TestFramework
{
    public class BaseTest : NsTestFrameworkUI.BaseTest
    {
        [TestInitialize]
        public virtual void Before()
        {
            KeyVaultHelper.InitializeVault("AppDev-Dev");
            Browser.InitializeDriver();
        }

        [TestCleanup]
        public virtual void After()
        {
            Browser.Cleanup();
            Browser.WebDriver.Dispose();
            Browser.WebDriver.Quit();
        }
        public void NavigateToUrl()
        {
            Browser.GoTo(Constants.BaseUrl);
        }
    }
}
