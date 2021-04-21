﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class AddAndDeleteNewFeatureToggle : BaseTest
    {       
        [TestMethod]
        [TestCategory("AddFeatureToggle")]
        [TestCategory("SmokeTests")]

        public void AddAndDeleteANewFeatureToggle_TheFeatureToggleIsAddedDeletedAnHasHistoryVisible()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.SmokeTestsApplication);
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.FeatureToggleName);

            //assert
            Pages.FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.FeatureToggleName).Should().BeTrue();
            Pages.FeatureTogglesPage.IsCreationDateCorrectlyDisplayed(Constants.FeatureToggleName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.EditFeatureToggle(Constants.FeatureToggleName);
            Pages.FeatureTogglesPage.DeleteToggleOnEdit(Constants.DeleteToggleOnEditReason);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            // act
            Pages.FeatureTogglesPage.AddFeatureToggle(Constants.NewFeatureToggleName);

            // assert
            Pages.FeatureTogglesPage.IsFeatureToggleDisplayed(Constants.NewFeatureToggleName).Should().BeTrue();

            // act
            Pages.FeatureTogglesPage.DeleteFeatureToggle(Constants.NewFeatureToggleName, Constants.DeleteToggleReason);

            //assert
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            // act 
            Pages.FeatureTogglesPage.GoToDeletedFeatureToggles();

            // assert
            Pages.FeatureTogglesPage.IsDeletedFeatureTogglesModalVisible();
            var deletedFeatureToggleNameFromGrid = Pages.FeatureTogglesPage.GetDeletedFeatureToggleNameFromGrid();
            deletedFeatureToggleNameFromGrid.Should().Be(Constants.NewFeatureToggleName);
        }

    }
}
