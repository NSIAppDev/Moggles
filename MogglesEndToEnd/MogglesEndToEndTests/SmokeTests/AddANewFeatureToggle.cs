using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesEndToEndTests.TestFramework;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class AddANewFeatureToggle
    {
        [TestInitialize]
        public void Before()
        {
            ChromeDriverUtils.KillChromeDriverProcesses();
            Browser.CreateNew(Constants.HeadlessModeEnabled);
        }

        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddANewFeatureToggle_TheFeatureToggleIsAdded()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.NewAddedFeatureToggleIsVisible(Constants.FeatureToggleName).Should().BeTrue();
            Pages.FeatureTogglesPage.CreationDateIsCorrectlyDisplayed(Constants.FeatureToggleName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();
        }

        [TestCleanup]
        public void After()
        {     
            Browser.Close();
            ChromeDriverUtils.KillChromeDriverProcesses();
        }
    }
}
