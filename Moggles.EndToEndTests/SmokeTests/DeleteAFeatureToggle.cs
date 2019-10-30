using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class DeleteAFeatureToggle : BaseTest
    {       
        [TestMethod]
        [TestCategory("DeleteFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void DeleteAFeatureToggle_TheFeatureToggleIsNoLongerVisible()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();
        }       
    }
}
