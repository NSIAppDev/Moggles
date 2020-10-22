using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;

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
            Pages.FeatureTogglesPage.Navigate();
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
            var featureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(FeatureFlagHandler.SmokeTestsApplicationId, Constants.FeatureToggleName);
            FeatureFlagHandler.DeleteFeatureToggles(FeatureFlagHandler.SmokeTestsApplicationId, featureToggleProperties.Id.ToString());
            base.After();
        }
    }
}