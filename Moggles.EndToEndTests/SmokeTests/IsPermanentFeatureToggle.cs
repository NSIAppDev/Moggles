using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class IsPermanentFeatureToggle : BaseTest
    {       
        [TestMethod]
        [TestCategory("IsPermanent")]
        [TestCategory("SmokeTests")]

        public void EditAFeatureToggleToBePermanent()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);

            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.SetFeatureToggleAsPermanent();

            //assert
            Pages.FeatureTogglesPage.FeatureToggleIsPermanent().Should().BeTrue();
        }

        [TestCleanup]
        public override void After()
        {
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.SetFeatureToggleAsPermanent();
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
            base.After();
        }
    }
}
