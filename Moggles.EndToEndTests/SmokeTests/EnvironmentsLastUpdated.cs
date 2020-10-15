using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using Moggles.Models;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;
using System.Linq;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class EnvironmentsLastUpdated : BaseTest
    {
        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddFeatureToggle_DevAndQaEnvironmentsAreUpdated()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsDevEnvironmentCheckboxChecked().Should().BeTrue();
            Pages.FeatureTogglesPage.IsQaEnvironmentCheckboxChecked().Should().BeTrue();

            Pages.FeatureTogglesPage.IsLastUpdatedDateOnDevCorrectlyDisplayed().Should().BeTrue();
            Pages.FeatureTogglesPage.IsLastUpdatedDateOnQaCorrectlyDisplayed().Should().BeTrue();
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
