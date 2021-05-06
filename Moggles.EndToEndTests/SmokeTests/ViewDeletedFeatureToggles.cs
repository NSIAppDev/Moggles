using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class ViewDeletedFeatureToggles : BaseTest
    {
        [TestInitialize]
        public override void Before()
        {
            base.Before();
            var applicationProperties = FeatureFlagHandler.GetApplicationProperties(Constants.SmokeTestsApplication);
            FeatureFlagHandler.AddFeatureToggles(applicationProperties.Id.ToString(), Constants.FeatureToggleName);
        }

        [TestMethod]
        [TestCategory("DeletedFeatureToggles")]
        [TestCategory("SmokeTests")]
        public void CheckFeatureTogglesThatWereDeletedAreVisibleInGrid()
        {
            // act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName, Constants.DeleteToggleReason);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            // act 
            Pages.FeatureTogglesPage.OpenDeletedFeatureTogglesSection();

            // assert
            Pages.FeatureTogglesPage.IsDeletedFeatureTogglesPanelVisible();
            var deletedFeatureToggleNameFromGrid = Pages.FeatureTogglesPage.GetDeletedFeatureToggleNameFromGrid();
            deletedFeatureToggleNameFromGrid.Should().Be(Constants.FeatureToggleName);
        }
    }
}
