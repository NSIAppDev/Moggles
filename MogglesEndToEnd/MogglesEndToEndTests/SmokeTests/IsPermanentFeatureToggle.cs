using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesEndToEndTests.TestFramework;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class IsPermanentFeatureToggle
    {
        [TestInitialize]
        public void Before()
        {
            ChromeDriverUtils.KillChromeDriverProcesses();
            Browser.CreateNew(Constants.HeadlessModeEnabled);
        }

        [TestMethod]
        [TestCategory("IsPermanent")]
        [TestCategory("SmokeTests")]

        public void EditAFeatureToggleToBePermanent()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);

            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.SetFeatureToggleAsPermanent();

            //assert
            Pages.FeatureTogglesPage.FeatureToggleIsPermanent().Should().BeTrue();
        }

        [TestCleanup]
        public void After()
        {
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.SetFeatureToggleAsPermanent();
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
            Browser.Close();
            ChromeDriverUtils.KillChromeDriverProcesses();
        }
    }
}
