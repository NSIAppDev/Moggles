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
            [Ignore]
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
                var featureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(FeatureFlagHandler.SmokeTestsApplicationId, Constants.FeatureToggleName);
                FeatureFlagHandler.DeleteFeatureToggles(FeatureFlagHandler.SmokeTestsApplicationId, featureToggleProperties.Id.ToString(), Constants.DeleteToggleReason);
                base.After();

            }
        }
}
