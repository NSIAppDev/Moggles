using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class RefreshEnvironment : BaseTest
    {
        [TestMethod]
        [TestCategory("RefreshEnvironment")]
        [TestCategory("SmokeTests")]

        public void UpdateDevEnvironment_DevEnvironmentIsRefreshed()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.UpdateDevEnvironment();
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsDevEnvironmentCheckboxChecked().Should().BeFalse();
        }

        [TestCleanup]
        public override void After()
        {
            Pages.FeatureTogglesPage.CloseEditFeatureFlagsModal();
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
            base.After();
        }
    }
}