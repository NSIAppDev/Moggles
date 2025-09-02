using Microsoft.Playwright;

namespace Moggles.E2EPlaywrightTests.Pages
{
    public class FeatureTogglesPage(IPage featureTogglesPage)
    {
        private readonly IPage _featureTogglesPage = featureTogglesPage;

    }
}
