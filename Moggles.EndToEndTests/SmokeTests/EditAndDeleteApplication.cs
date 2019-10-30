using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.TestFramework;
using MogglesEndToEndTests.TestFramework;
using NSTestFrameworkDotNetCoreUI.Helpers;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class EditAndDeleteApplication : BaseTest
    {       
        [TestMethod]
        [TestCategory("EditANewApplication")]
        [TestCategory("SmokeTests")]

        public void EditAndDeleteApplication_TheApplicationIsUpdatedAndAfterThatDeleted()
        {
            //act
            Browser.Goto(Constants.BaseUrl);
            Pages.FeatureTogglesPage.AddNewApplication(Constants.NewApplicationName, Constants.FirstEnvName);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.NewApplicationName);
            Pages.FeatureTogglesPage.ChangeApplicationName(Constants.NewApplicationName,Constants.EditedApplicationName);

            //assert
            Pages.FeatureTogglesPage.ApplicationIsSelected(Constants.EditedApplicationName);
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteApplication(Constants.EditedApplicationName);

            //assert
            Pages.FeatureTogglesPage.ApplicationNameExists(Constants.EditedApplicationName).Should().BeFalse();
        }
    }
}
