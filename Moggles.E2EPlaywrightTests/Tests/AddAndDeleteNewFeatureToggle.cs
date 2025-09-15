using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests;

[TestClass]
public class AddAndDeleteNewFeatureToggle : BaseTest
{
    private static string FeatureToggleName = Constants.FeatureToggleName;

    [TestMethod]
    [TestCategory("AddFeatureToggle")]
    [TestCategory("SmokeTests")]
    [TestProperty("role","admin")]

    public async Task AddAndDeleteANewFeatureToggle_TheFeatureToggleIsAddedAndDeleted()
    {
        await _page.GotoAsync(Constants.BaseUrl);
        await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
        await FeatureTogglesPage.AddFeatureToggle(FeatureToggleName);

        //assert
        (await FeatureTogglesPage.IsFeatureToggleDisplayed(FeatureToggleName)).Should().BeTrue();
        (await FeatureTogglesPage.IsCreationDateCorrectlyDisplayed(FeatureToggleName)).Should().BeTrue();

        //act
        await FeatureTogglesPage.EditFeatureToggle(FeatureToggleName);
        await FeatureTogglesPage.DeleteToggleOnEdit(Constants.DeleteToggleReason);

        //assert
        (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();
    }
    [TestCleanup]
    public override async Task TeardownAsync()
    {
        var appId = await FeatureFlagHelper.GetSmokeTestsApplicationIdAsync(Constants.SmokeTestsApplication);
        try
        {
            var featureToggleProperties = await FeatureFlagHelper.GetFeatureToggleProperties(appId, FeatureToggleName);
            await FeatureFlagHelper.DeleteFeatureToggles(
                appId,
                featureToggleProperties.Id.ToString(),
                Constants.DeleteToggleReason);
        }
        catch (Exception ex) { 
            return; 
        }
        await base.TeardownAsync();
    }
}
