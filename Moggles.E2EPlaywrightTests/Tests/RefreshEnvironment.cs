using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class RefreshEnvironment : BaseTest
    {
        [TestMethod]
        [TestCategory("RefreshEnvironment")]
        [TestCategory("SmokeTests")]

        public async Task UpdateEnvironment_RefreshIsTriggered()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            await FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
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
                featureToggleProperties.Id.ToString(),
                Constants.DeleteToggleReason);
            await base.TeardownAsync();
        }
    }
}