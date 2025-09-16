using AwesomeAssertions;
using Moggles.E2EPlaywrightTests.Helpers;
using Moggles.E2EPlaywrightTests.Helpers.Models;
using Moggles.Models;

namespace Moggles.E2EPlaywrightTests.Tests
{
    [TestClass]
    public class EditAndDeleteApplication : BaseTest
    {
        private ApplicationDto ApplicationInfo;
        private string ApplicationToDelete = "AppForDeletion";
        private string EditedApplicationToDelete = "EditedAppForDeletion";

        [TestInitialize]
        public override async Task SetupAsync()
        {
            await base.SetupAsync();
            if(await FeatureFlagHelper.DoesApplicationExists(ApplicationToDelete) == true)
            {
                ApplicationInfo = await FeatureFlagHelper.GetApplicationProperties(ApplicationToDelete);
                var body = new UpdateApplicationModel
                {
                    ApplicationName = ApplicationInfo.AppName,
                    Id = ApplicationInfo.Id,
                    isDeleted = false
                };
                await FeatureFlagHelper.ReactivateApp(body);
            }
            else
            {
                ApplicationInfo = await FeatureFlagHelper.GetApplicationProperties(EditedApplicationToDelete);
                var body = new UpdateApplicationModel
                {
                    ApplicationName = ApplicationToDelete,
                    Id = ApplicationInfo.Id,
                    isDeleted = false
                };
                await FeatureFlagHelper.UpdateApplicationProperties(body);
            }
        }

        [TestMethod, TestCategory("EditANewApplication"), TestCategory("SmokeTests")]
        [Description("Check soft delete for test. Message in UI should be visible when adding app with the same name as existing (and deleted) one")]
        [TestProperty("role", "admin")]
        public async Task EditAndDeleteApplication_TheApplicationIsUpdatedAndAfterThatDeleted()
        {
            //act
            await _page.GotoAsync(Constants.BaseUrl);

            await FeatureTogglesPage.SelectApplicationByName(ApplicationToDelete);
            await FeatureTogglesPage.ChangeApplicationName(ApplicationToDelete, EditedApplicationToDelete);

            //assert
            (await FeatureTogglesPage.GetSelectedApplicationName(EditedApplicationToDelete)).Equals(EditedApplicationToDelete).Should().BeTrue();
            (await FeatureTogglesPage.IsGridEmpty()).Should().BeTrue();

            //act
            await FeatureTogglesPage.ChangeApplicationName(EditedApplicationToDelete, ApplicationToDelete);

            //assert
            (await FeatureTogglesPage.IsApplicationListed(EditedApplicationToDelete)).Should().BeFalse();
        }

        [TestCleanup]
        public override async Task TeardownAsync()
        {
            if (await FeatureFlagHelper.DoesApplicationExists(EditedApplicationToDelete) == true)
            {
                var body = new UpdateApplicationModel
                {
                    ApplicationName =ApplicationToDelete,
                    Id = ApplicationInfo.Id,
                    isDeleted = false
                };
                await FeatureFlagHelper.UpdateApplicationProperties(body);
            }
            await base.TeardownAsync();
        }
    }
}
