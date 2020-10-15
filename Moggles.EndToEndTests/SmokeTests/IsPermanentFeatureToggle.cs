using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using Moggles.Models;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
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
            Browser.Goto(Constants.BaseUrl);
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
            var FeatureToggleProperties = FeatureFlagHandler.GetFeatureToggleProperties(Constants.ApplicationId, Constants.FeatureToggleName);
            _featureToggleUpdateModel = FeatureFlagHandler.SetFeatureToggleUpdateModel(FeatureToggleProperties, Constants.ApplicationId, false, Constants.SecondEnvName);
            FeatureFlagHandler.UpdateFeatureFlag(_featureToggleUpdateModel);
            FeatureFlagHandler.DeleteFeatureToggles(Constants.ApplicationId, FeatureToggleProperties.Id.ToString());
            base.After();
        }
    }
}
