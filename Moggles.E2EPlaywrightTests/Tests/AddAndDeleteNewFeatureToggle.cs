using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests;

[TestClass]
public class AddAndDeleteNewFeatureToggle : BaseTest
{
    [TestMethod]
    [TestCategory("AddFeatureToggle")]
    [TestCategory("SmokeTests")]
    [TestProperty("role","admin")]

    public async Task AddAndDeleteANewFeatureToggle_TheFeatureToggleIsAddedAndDeleted()
    {
        await _page.GotoAsync(Constants.BaseUrl);
        await FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
        await FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);

        //assert
        (await FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.FeatureToggleName)).Should().BeTrue();
        (await FeatureTogglesPage.IsCreationDateCorrectlyDisplayed(Constants.FeatureToggleName)).Should().BeTrue();

        //act
        await FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
        await FeatureTogglesPage.DeleteToggleOnEdit(Constants.DeleteToggleReason);

        //assert
        (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();
    }
}
