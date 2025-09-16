using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class RefreshEnvironment : BaseTest
    {
        private static string FeatureToggleName = Constants.FeatureToggleName;

        [TestMethod]
        [TestCategory("RefreshEnvironment")]
        [TestCategory("SmokeTests")]
        [TestProperty("role", "admin")]
        public async Task UpdateEnvironment_RefreshIsTriggered()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.AddFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.EditFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.UpdateDevEnvironment();
            await FeatureTogglesPage.RefreshEnvironment();

            //assert
            (await FeatureTogglesPage.IsRefreshedEnvironmentMessageIsDisplayed()).Should().BeTrue();
        }

        [TestCleanup]
        public override async Task TeardownAsync()
        {
            var appId = await FeatureFlagHelper.GetSmokeTestsApplicationIdAsync(Constants.SmokeTestsApplication);
            var featureToggleProperties = await FeatureFlagHelper.GetFeatureToggleProperties(appId, FeatureToggleName);

            await FeatureFlagHelper.DeleteFeatureToggles(
                appId,
                featureToggleProperties.Id,
                Constants.DeleteToggleReason);
            await base.TeardownAsync();
        }
    }
}