using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsTestFrameworkUI.Helpers;

namespace Moggles.EndToEndTests.TestFramework
{
    public class BaseTest : NsTestFrameworkUI.BaseTest
    {
        [TestInitialize]
        public virtual void Before()
        {
            Browser.InitializeDriver();
        }

        [TestCleanup]
        public virtual void After()
        {
            Browser.Cleanup();
            Browser.WebDriver.Dispose();
            Browser.WebDriver.Quit();
        }
    }
}
