using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class EnvironmentsLastUpdated : BaseTest
    {
        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddFeatureToggle_DevAndQaEnvironmentsAreUpdated()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsDevEnvironmentCheckboxChecked().Should().BeTrue();
            Pages.FeatureTogglesPage.IsQaEnvironmentCheckboxChecked().Should().BeTrue();

            Pages.FeatureTogglesPage.IsLastUpdatedDateOnDevCorrectlyDisplayed().Should().BeTrue();
            Pages.FeatureTogglesPage.IsLastUpdatedDateOnQaCorrectlyDisplayed().Should().BeTrue();
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
