using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
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
                Pages.FeatureTogglesPage.Navigate();
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
                var featureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(Constants.ApplicationId, Constants.FeatureToggleName);
                FeatureFlagHandler.DeleteFeatureToggles(Constants.ApplicationId, featureToggleProperties.Id.ToString());
                base.After();

            }
        }
}
