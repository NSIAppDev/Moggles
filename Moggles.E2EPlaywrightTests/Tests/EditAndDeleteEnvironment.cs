using Moggles.Models;
using Moggles.E2EPlaywrightTests.Helpers;
using AwesomeAssertions;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class EditAndDeleteEnvironment : BaseTest
    {
        [TestInitialize]
        public override async Task SetupAsync()
        {
            await base.SetupAsync();
            var applicationInfo = await FeatureFlagHelper.GetApplicationProperties(Constants.NewApplicationName);
            var body = new UpdateApplicationModel
            {
                ApplicationName = applicationInfo.AppName,
                Id = applicationInfo.Id,
                isDeleted = false
            };
            await FeatureFlagHelper.ReactivateApp(body);
            await FeatureFlagHelper.DeleteFeatureToggleEnvironment(applicationInfo.Id.ToString(), Constants.SecondEnvName);
            await FeatureFlagHelper.DeleteFeatureToggleEnvironment(applicationInfo.Id.ToString(), Constants.EditedSecondEnvName);
        }

        [TestProperty("role", "admin")]
        [TestMethod, TestCategory("EditANewEnvironment")]
        [TestCategory("SmokeTests")]

        public async Task EditAndDeleteEnvironment_TheEnvironmentIsUpdatedAndAfterThatDeleted()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);
            await FeatureTogglesPage.SelectApplicationByName(Constants.NewApplicationName);
            await FeatureTogglesPage.AddNewEnvironment(Constants.SecondEnvName);
            await FeatureTogglesPage.EditEnvironment(Constants.SecondEnvName);
            await FeatureTogglesPage.ChangeEnvironmentName(Constants.EditedSecondEnvName);

            //assert
            (await FeatureTogglesPage.IsEnvironmentNameDisplayed(Constants.EditedSecondEnvName)).Should().BeTrue();

            //act
            await FeatureTogglesPage.DeleteEnvironment(Constants.EditedSecondEnvName);

            //assert
            (await FeatureTogglesPage.IsEnvironmentNameNotDisplayed(Constants.EditedSecondEnvName)).Should().BeTrue();
        }

        [TestCleanup]
        public override async Task TeardownAsync()
        {
            var applicationProperties = await FeatureFlagHelper.GetApplicationProperties(Constants.NewApplicationName);
            await FeatureFlagHelper.DeleteApplication(applicationProperties.Id.ToString());
            await base.TeardownAsync();
        }

    }
}
