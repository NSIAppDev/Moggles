using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
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
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.FeatureToggleName).Should().BeTrue();
            Pages.FeatureTogglesPage.IsCreationDateCorrectlyDisplayed(Constants.FeatureToggleName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
            
            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();
        }

    }
}
