using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moggles.EndToEndTests.MogglesPages;
using Moggles.EndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;

namespace Moggles.EndToEndTests.SmokeTests
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
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.AddNewApplication(Constants.NewApplicationName, Constants.FirstEnvName);
            Pages.FeatureTogglesPage.ChangeApplicationName(Constants.NewApplicationName,Constants.EditedApplicationName);

            //assert
            Pages.FeatureTogglesPage.GetSelectedApplicationName().Equals(Constants.EditedApplicationName).Should().BeTrue();
            Pages.FeatureTogglesPage.IsGridEmpty().Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteApplication(Constants.EditedApplicationName);

            //assert
            Pages.FeatureTogglesPage.IsApplicationListed(Constants.EditedApplicationName).Should().BeFalse();
        }
    }
}
