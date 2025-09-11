using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class IsAcceptedByUserFeatureToggle : BaseTest
    {
        private static string FeatureToggleName = Constants.FeatureToggleName;

        [TestMethod, TestCategory("SmokeTests"), TestCategory("IsAcceptedByUser")]
        [TestProperty("role", "admin")]
        public async Task EditAFeatureToggleToBeAcceptedByUser()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.AddFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.EditFeatureToggle(FeatureToggleName);
            await FeatureTogglesPage.SetFeatureToggleAsAcceptedByUser();

            //assert
            (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();

            //act
            await FeatureTogglesPage.FilterAcceptedByUserColumn(Constants.AcceptedByUserStatus);

            //assert
            (await FeatureTogglesPage.IsFeatureToggleDisplayed(FeatureToggleName)).Should().BeTrue();
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
