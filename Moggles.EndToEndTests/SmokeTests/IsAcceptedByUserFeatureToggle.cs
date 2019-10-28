﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;

namespace MogglesEndToEndTests.SmokeTests
{
        [TestClass]
        public class IsAcceptedByUserFeatureToggle : BaseTest
        {          
            [TestMethod]
            [Ignore]
            [TestCategory("IsAcceptedByUser")]
            [TestCategory("SmokeTests")]

            public void EditAFeatureToggleToBeAcceptedByUser()
            {
                //act
                Pages.FeatureTogglesPage.Navigate();
                Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.SmokeTestsApplication);
                Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);
                Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
                Pages.FeatureTogglesPage.SetFeatureToggleAsAcceptedByUser();

                //assert
                Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

                //act
                Pages.FeatureTogglesPage.FilterByAcceptedByUser(Constants.AcceptedByUserStatus);

                //assert
                Pages.FeatureTogglesPage.NewAddedFeatureToggleIsVisible(Constants.FeatureToggleName).Should().BeTrue();
            }

            [TestCleanup]
            public override void After()
            {
                Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.FeatureToggleName);
                base.After();
            }
        }
}