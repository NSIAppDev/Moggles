using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class ViewDeletedFeatureToggles : BaseTest
    {
        [TestInitialize]
        public override async Task SetupAsync()
        {
            await base.SetupAsync();
            var applicationInfo = await FeatureFlagHelper.GetApplicationProperties(Constants.SmokeTestsApplication);

            await FeatureFlagHelper.AddFeatureToggles(applicationInfo.Id.ToString(), Constants.FeatureToggleName);
        }

        [TestMethod]
        [TestCategory("DeletedFeatureToggles")]
        [TestCategory("SmokeTests")]
        public async Task CheckFeatureTogglesThatWereDeletedAreVisibleInGrid()
        {
            // act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            await FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName, Constants.DeleteToggleReason);

            //assert
            (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();

            // act 
            await FeatureTogglesPage.OpenDeletedFeatureTogglesSection();

            // assert
            await FeatureTogglesPage.IsDeletedFeatureTogglesPanelVisible();
            var deletedFeatureToggleNameFromGrid = await FeatureTogglesPage.GetDeletedFeatureToggleNameFromGrid();
            deletedFeatureToggleNameFromGrid.Should().Be(Constants.FeatureToggleName);
        }

        [TestCleanup]
        public override async Task TeardownAsync()
        {
            await FeatureTogglesPage.RemoveAllDeletedFeatureToggles();
            await base.TeardownAsync();
        }
    }
}
