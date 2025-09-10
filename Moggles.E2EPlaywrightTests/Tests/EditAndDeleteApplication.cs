using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;
using Moggles.Models;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class EditAndDeleteApplication : BaseTest
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
        }


        [TestMethod, TestCategory("EditANewApplication"), TestCategory("SmokeTests")]
        [Description("Check soft delete for test. Message in UI should be visible when adding app with the same name as existing (and deleted) one")]
        [TestProperty("role", "admin")]
        public async Task EditAndDeleteApplication_TheApplicationIsUpdatedAndAfterThatDeleted()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(Constants.NewApplicationName);
            await FeatureTogglesPage.ChangeApplicationName(Constants.NewApplicationName, Constants.EditedApplicationName);

            //assert
            (await FeatureTogglesPage.GetSelectedApplicationName()).Equals(Constants.EditedApplicationName).Should().BeTrue();
            (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();

            //act
            await FeatureTogglesPage.ChangeApplicationName(Constants.EditedApplicationName, Constants.NewApplicationName);

            //assert
            (await FeatureTogglesPage.IsApplicationListed(Constants.EditedApplicationName)).Should().BeFalse();
        }
    }
}
