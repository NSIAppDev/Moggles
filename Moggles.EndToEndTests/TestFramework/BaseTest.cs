using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSTestFrameworkDotNetCoreUI.Helpers;
using System.IO;
using System.Reflection;

namespace Moggles.EndToEndTests.TestFramework
{
    public class BaseTest : NSTestFrameworkDotNetCoreUI.BaseTest
    {
        [TestInitialize]
        public virtual void Before()
        {
            var chromeDriverPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Browser.InitializeDriver(chromeDriverPath);
        }
    }
}
