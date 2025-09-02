using Moggles.E2EPlaywrightTests.Helpers;

namespace Moggles.E2EPlaywrightTests.Tests;

[TestClass]
public class Test1 : BaseTest
{
    [TestMethod]
    [TestCategory("AddFeatureToggle")]
    [TestCategory("SmokeTests")]
    [TestProperty("role","admin")]

    public async Task AddAndDeleteANewFeatureToggle_TheFeatureToggleIsAddedAndDeleted()
    {
        await _page.GotoAsync(Constants.BaseUrl);
        var title = await _page.TitleAsync();

    }
}
