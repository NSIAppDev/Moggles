using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.Helpers;
using Moggles.EndToEndTests.TestFramework;
using Moggles.Models;

namespace Moggles.EndToEndTests.SmokeTests
{
    [TestClass]
    public class EditAndDeleteApplication : BaseTest
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
        }

        [TestMethod]
        [TestCategory("EditANewApplication")]
        [TestCategory("SmokeTests")]
        [Description("Check soft delete for test. Message in UI should be visible when adding app with the same name as existing (and deleted) one")]

        public void EditAndDeleteApplication_TheApplicationIsUpdatedAndAfterThatDeleted()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();

            Pages.FeatureTogglesPage.SelectApplicationByName(Constants.NewApplicationName);
            Pages.FeatureTogglesPage.ChangeApplicationName(Constants.NewApplicationName,Constants.EditedApplicationName);

            //assert
            Pages.FeatureTogglesPage.GetSelectedApplicationName().Equals(Constants.EditedApplicationName).Should().BeTrue();
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.ChangeApplicationName(Constants.EditedApplicationName, Constants.NewApplicationName);

            //assert
            Pages.FeatureTogglesPage.IsApplicationListed(Constants.EditedApplicationName).Should().BeFalse();
        }
    }
}
