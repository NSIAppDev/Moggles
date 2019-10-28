﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;

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
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
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
            Pages.FeatureTogglesPage.CloseEditFeatureFlagsModal();
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
            base.After();
        }
    }
}
