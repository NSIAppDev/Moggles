using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using Moggles.Models;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class EditAndDeleteEnvironment : BaseTest
    {
        [TestInitialize]
        public override void Before()
        {
            base.Before();
            var applicationInfo = FeatureFlagHandler.GetApplicationProperties(Constants.NewApplicationName);
            var body = new UpdateApplicationModel
            {
                ApplicationName = applicationInfo.AppName,
                Id = applicationInfo.Id,
                isDeleted = false
            };
            FeatureFlagHandler.ReactivateApp(body);
            FeatureFlagHandler.DeleteFeatureToggleEnvironment(applicationInfo.Id.ToString(), Constants.SecondEnvName);
        }

        [TestMethod]   
        [TestCategory("EditANewEnvironment")]
        [TestCategory("SmokeTests")]

        public void EditAndDeleteEnvironment_TheEnvironmentIsUpdatedAndAfterThatDeleted()
        {
            //act
            NavigateToUrl();
            Pages.AuthenticationPage.Login();

            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.NewApplicationName);
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
