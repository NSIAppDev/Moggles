using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class ViewDeletedFeatureToggles : BaseTest
    {
        private Guid AppId;
        private string FeatureToggleName = Constants.FeatureToggleName;

        [TestInitialize]
        public override async Task SetupAsync()
        {
            await base.SetupAsync();
            AppId = await FeatureFlagHelper.GetSmokeTestsApplicationIdAsync(Constants.SmokeTestsApplication);
            await FeatureFlagHelper.AddFeatureToggles(AppId, FeatureToggleName);
        }

        [TestMethod]
        [TestCategory("DeletedFeatureToggles")]
        [TestCategory("SmokeTests")]
        [TestProperty("role", "admin")]
        public async Task CheckFeatureTogglesThatWereDeletedAreVisibleInGrid()
        {
            // act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.DeleteFeatureToggle(FeatureToggleName, Constants.DeleteToggleReason);

            //assert
            (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();

            // act 
            await FeatureTogglesPage.OpenDeletedFeatureTogglesSection();

            // assert
            await FeatureTogglesPage.IsDeletedFeatureTogglesPanelVisible();
            var deletedFeatureToggleNameFromGrid = await FeatureTogglesPage.GetDeletedFeatureToggleNameFromGrid();
            deletedFeatureToggleNameFromGrid.Should().Be(FeatureToggleName);
        }

        [TestCleanup]
        public override async Task TeardownAsync()
        {
            var featureToggleId = (await FeatureFlagHelper.GetDeletedFeatureToggleProperties(AppId, FeatureToggleName)).Id;
            await FeatureFlagHelper.DeleteFeatureTogglesFromHistory(AppId, featureToggleId);
            await base.TeardownAsync();
        }
    }
}
