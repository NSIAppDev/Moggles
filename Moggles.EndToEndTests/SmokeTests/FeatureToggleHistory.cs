using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;
using Moggles.Models;
using System;
using NSTestFrameworkDotNetCoreApi.RestSharp;
using NSTestFrameworkDotNetCoreApi.RestSharp.model;


namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class FeatureToggleHistory : BaseTest
    {
        [TestInitialize]
        public override void Before()
        {
           var featureToggle = new FeatureToggleAddModel
            {
                ApplicationId = "1e6ba677-2af3-446d-b747-476542fca042",
                FeatureToggleName = Constants.FeatureToggleName,
                Notes = "",
                IsPermanent = false
            };
           Test.AddFeatureFlagg(featureToggle);
            base.Before();
        }


        [TestMethod]
        [TestCategory("SmokeTests")]
        public void UpdateFeatureToggle_TheHistoryIsUpdated()
        {
            //act 
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);

        }
    }
}
