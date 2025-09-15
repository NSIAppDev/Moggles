using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class IsPermanentFeatureToggle : BaseTest
    {
        private static string FeatureToggleName = Constants.FeatureToggleName;

        [TestMethod]
        [TestCategory("IsPermanent")]
        [TestCategory("SmokeTests")]

        public async Task EditAFeatureToggleToBePermanent()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.AddFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.EditFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.SetFeatureToggleAsPermanent();

            //assert
            (await FeatureTogglesPage.IsFeatureTogglePermanent()).Should().BeTrue();
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

            await base.TeardownAsync();
        }

    }
}
