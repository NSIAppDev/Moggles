using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class AddAndDeleteNewFeatureToggle : BaseTest
    {       
        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddAndDeleteANewFeatureToggle_TheFeatureToggleIsAddedAndDeleted()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.FeatureToggleName).Should().BeTrue();
            Pages.FeatureTogglesPage.IsCreationDateCorrectlyDisplayed(Constants.FeatureToggleName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.DeleteToggleOnEdit(Constants.DeleteToggleReason);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

        }

    }
}
