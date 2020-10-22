using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;

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
                var applicationProperties = FeatureFlagHandler.GetApplicationProperties(Constants.SmokeTestsApplication);
                var applicationId = applicationProperties.Id.ToString();
                var featureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(applicationId, Constants.FeatureToggleName);
                FeatureFlagHandler.DeleteFeatureToggles(applicationId, featureToggleProperties.Id.ToString());
                base.After();

            }
        }
}
