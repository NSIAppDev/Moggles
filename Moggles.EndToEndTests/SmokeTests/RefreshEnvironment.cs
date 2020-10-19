using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class RefreshEnvironment : BaseTest
    {
        [TestMethod]
        [TestCategory("RefreshEnvironment")]
        [TestCategory("SmokeTests")]

        public void UpdateEnvironment_RefreshIsTriggered()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.UpdateDevEnvironment();
            Pages.FeatureTogglesPage.RefreshEnvironment();

            //assert
            Pages.FeatureTogglesPage.IsRefreshedEnvironmentMessageIsDisplayed().Should().BeTrue();
        }

        [TestCleanup]
        public override void After()
        {
            var FeatureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(Constants.ApplicationId, Constants.FeatureToggleName);
            FeatureFlagHandler.DeleteFeatureToggles(Constants.ApplicationId, FeatureToggleProperties.Id.ToString());
            base.After();
        }
    }
}