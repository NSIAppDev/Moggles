using Microsoft.VisualStudio.TestTools.UnitTesting;
using NsTestFrameworkUI.Helpers;
using System.IO;
using System.Reflection;

namespace Moggles.EndToEndTests.TestFramework
{
    public class BaseTest : NsTestFrameworkUI.BaseTest
    {
        [TestInitialize]
        public virtual void Before()
        {
            var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Browser.InitializeDriver(chromeDriverPath);
        }
    }
}
