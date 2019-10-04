using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MogglesEndToEndTests.TestFramework;

namespace MogglesEndToEndTests.SmokeTests
{
    [TestClass]
    public class EditAndDeleteEnvironment
    {
        [TestInitialize]
        public void Before()
        {
            ChromeDriverUtils.KillChromeDriverProcesses();
            Browser.CreateNew(Constants.HeadlessModeEnabled);
        }

        [TestMethod]
        
        [TestCategory("EditANewEnvironment")]
        [TestCategory("SmokeTests")]

        public void EditAndDeleteEnvironment_TheEnvironmentIsUpdatedAndAfterThatDeleted()
        {
            //act
            Pages.FeatureTogglesPage.Navigate();
            Pages.FeatureTogglesPage.AddNewApplication(Constants.NewApplicationName, Constants.FirstEnvName);
            Pages.FeatureTogglesPage.SelectASpecificApplication(Constants.NewApplicationName);
            Pages.FeatureTogglesPage.AddNewEnvironment(Constants.SecondEnvName);
            Pages.FeatureTogglesPage.EditEnvironment(Constants.SecondEnvName);
            Pages.FeatureTogglesPage.ChangeEnvironmentName(Constants.EditedSecondEnvName);

            //assert
            Pages.FeatureTogglesPage.EnvironmentNameExist(Constants.EditedSecondEnvName).Should().BeTrue();

            //act
            Pages.FeatureTogglesPage.DeleteEnvironment(Constants.EditedSecondEnvName);

            //assert
            Pages.FeatureTogglesPage.EnvironmentNameExist(Constants.EditedSecondEnvName).Should().BeFalse();
        }

        [TestCleanup]
        public void After()
        {
            Pages.FeatureTogglesPage.DeleteApplication(Constants.NewApplicationName);
            Browser.Close();
            ChromeDriverUtils.KillChromeDriverProcesses();
        }
    }
}
