using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class EditAndDeleteEnvironment : BaseTest
    {      
        [TestMethod]   
        [TestCategory("EditANewEnvironment")]
        [TestCategory("SmokeTests")]

        public void EditAndDeleteEnvironment_TheEnvironmentIsUpdatedAndAfterThatDeleted()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.AddNewApplication(Constants.NewApplicationName, Constants.FirstEnvName);
            Pages.FeatureTogglesPage.AddNewEnvironment(Constants.SecondEnvName);
            Pages.FeatureTogglesPage.EditEnvironment(Constants.SecondEnvName);
            Pages.FeatureTogglesPage.ChangeEnvironmentName(Constants.EditedSecondEnvName);

            //assert
            Pages.FeatureTogglesPage.IsEnvironmentNameDisplayed(Constants.EditedSecondEnvName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteEnvironment(Constants.EditedSecondEnvName);

            //assert
            Pages.FeatureTogglesPage.IsEnvironmentNameDisplayed(Constants.EditedSecondEnvName).Should().BeFalse();
        }

        [TestCleanup]
        public override void After()
        {
            var applicationProperties = FeatureFlagHandler.GetApplicationProperties(Constants.NewApplicationName);
            FeatureFlagHandler.DeleteApplication(applicationProperties.Id.ToString());
            base.After();
        }
    }
}
