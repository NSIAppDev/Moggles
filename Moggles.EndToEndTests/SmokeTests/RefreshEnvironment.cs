using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class RefreshEnvironment
    {
        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddANewFeatureToggle_TheFeatureToggleIsAdded()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.UpdateDevEnvironment();
        }
    }
}