using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
{
        [TestClass]
        public class IsAcceptedByUserFeatureToggle : BaseTest
        {          
            [TestMethod]
            [TestCategory("IsAcceptedByUser")]
            [TestCategory("SmokeTests")]

            public void EditAFeatureToggleToBeAcceptedByUser()
            {
            //act
                Browser.Goto(Constants.BaseUrl);
                Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
                Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
                Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
                Pages.FeatureTogglesPage.SetFeatureToggleAsAcceptedByUser();

                //assert
                Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

                //act
                Pages.FeatureTogglesPage.FilterAcceptedByUserColumn(Constants.AcceptedByUserStatus);

                //assert
                Pages.FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.FeatureToggleName).Should().BeTrue();
            }

            [TestCleanup]
            public override void After()
            {
                var FeatureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(Constants.ApplicationId, Constants.FeatureToggleName);
                FeatureFlagHandler.DeleteFeatureToggles(Constants.ApplicationId, FeatureToggleProperties.Id.ToString());
                base.After();

            }
        }
}
