using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using Moggles.Models;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class IsPermanentFeatureToggle : BaseTest
    {
        private static FeatureToggleUpdateModel _featureToggleUpdateModel;

        [TestMethod]
        [TestCategory("IsPermanent")]
        [TestCategory("SmokeTests")]

        public void EditAFeatureToggleToBePermanent()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.SetFeatureToggleAsPermanent();

            //assert
            Pages.FeatureTogglesPage.IsFeatureTogglePermanent().Should().BeTrue();
        }

        [TestCleanup]
        public override void After()
        {
            var applicationProperties = FeatureFlagHandler.GetApplicationProperties(Constants.SmokeTestsApplication);
            var applicationId = applicationProperties.Id.ToString();
            var featureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(applicationId, Constants.FeatureToggleName);
            _featureToggleUpdateModel = FeatureFlagHandler.SetFeatureToggleUpdateModel(featureToggleProperties, applicationId, false, Constants.SecondEnvName);
            FeatureFlagHandler.UpdateFeatureFlag(_featureToggleUpdateModel);
            FeatureFlagHandler.DeleteFeatureToggles(applicationId, featureToggleProperties.Id.ToString());
            base.After();
        }
    }
}
