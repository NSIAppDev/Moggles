using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class ViewDeletedFeatureToggles : BaseTest
    {
        [TestMethod]
        [TestCategory("DeletedFeatureToggles")]
        [TestCategory("SmokeTests")]
        public void CheckFeatureTogglesThatWereDeletedAreVisibleInGrid()
        {
            // act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.DeleteToggleOnEdit(Constants.DeleteToggleOnEditReason);
            Pages.FeatureTogglesPage.GoToDeletedFeatureToggles();

            // assert
            Pages.FeatureTogglesPage.IsDeletedFeatureTogglesModalVisible();
            var deletedFeatureToggleNameFromGrid = Pages.FeatureTogglesPage.GetDeletedFeatureToggleNameFromGrid();
            deletedFeatureToggleNameFromGrid.Should().Be(Constants.FeatureToggleName);

        }
    }
}
