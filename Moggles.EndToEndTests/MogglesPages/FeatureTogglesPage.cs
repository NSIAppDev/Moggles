using MogglesEndToEndTests.TestFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using NSTestFrameworkDotNetCoreUI.Helpers;
using NSTestFrameworkDotNetCoreUI.Pages;

namespace MogglesEndToEndTests.MogglesPages
{
    public class FeatureTogglesPage
    {
        public IWebElement ToolsMenuDropdown =>
            Browser.WebDriver.FindElement(By.CssSelector("#bs-example-navbar-collapse-1 > ul > li > ul"));

        public IWebElement ToolsButton => Browser.WebDriver.FindElement(By.Id("toolsBtn"));

        public IWebElement FeatureToggleNameInput => Browser.WebDriver.FindElement(By.Id("featureToggleName"));
    
        public IWebElement NotesInput => Browser.WebDriver.FindElement(By.Id("notesInput"));

        public IWebElement AddFeatureToggleButton => Browser.WebDriver.FindElement(By.Id("addFeatureToggleBtn"));

        public IWebElement AddApplicationButton => Browser.WebDriver.FindElement(By.Id("addApplicationBtn"));
                        
        public IWebElement AddEnvironmentButton => Browser.WebDriver.FindElement(By.Id("addEnvironmentBtn"));

        public IWebElement CloseAddEnvironmentModalBtn => Browser.WebDriver.FindElement(By.Id("closeAddEnvironmentModalBtn"));
        public IWebElement CloseAddToggleModalBtn => Browser.WebDriver.FindElement(By.Id("closeAddToggleModalBtn"));
        public IWebElement CloseAddApplicationModalBtn => Browser.WebDriver.FindElement(By.Id("closeAddApplicationModalBtn"));
        public IWebElement FeatureTogglesGrid => Browser.WebDriver.FindElement(By.Id("toggleGrid"));

        public IWebElement DeleteFeatureToggleButton => Browser.WebDriver.FindElement(By.Id("deleteToggleBtn"));

        public IWebElement IsPermanentCheckbox =>
            Browser.WebDriver.FindElement(By.Id("editIsPermanentCheckbox"));

        public IWebElement DevEnvironmentCheckbox =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "div:nth-child(2) > div.col-sm-1 > div > div > input[type=checkbox]"));

        public IWebElement RefreshEnvironmentButton =>Browser.WebDriver.FindElement(By.Id("refreshEnvironmentsBtn"));

        public IWebElement FilterByACriteria =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div > div> div > div > div > div> div > div> table > thead > tr:nth-child(2) > th:nth-child(2) > div > input"));

        public IWebElement IsAcceptedByUserCheckbox => Browser.WebDriver.FindElement(By.Id("editAcceptedByUserCheckbox"));

        public IWebElement SaveButton => Browser.WebDriver.FindElement(By.Id("saveEditToggleBtn"));

        public IWebElement SelectApplication =>
            Browser.WebDriver.FindElement(By.Id("selectedApp"));

        public IWebElement ApplicationNameInput => Browser.WebDriver.FindElement(By.Id("addApplicationNameInput"));

        public IWebElement ApplicationsDropdown =>
            Browser.WebDriver.FindElement(By.CssSelector("#selectedApp>ul"));

        public IWebElement FirstEnvNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("div.in > div > div > div > div > div > div > div:nth-child(2) > div > input"));

        public IWebElement SecondEnvNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("div.in > div > div > div > div > div > div > div:nth-child(1) > div > input"));

        public IWebElement EditApplicationIcon => Browser.WebDriver.FindElement(By.Id("showEditApplicationModalBtn"));

        public IWebElement EditApplicationNameInput => Browser.WebDriver.FindElement(By.Id("editApplicationNameInput"));

        public IWebElement SaveApplicationChangesButton => Browser.WebDriver.FindElement(By.Id("saveEditApplicationBtn"));

        public IWebElement DeleteApplicationButton => Browser.WebDriver.FindElement(By.Id("deleteApplicationBtn"));

        public IWebElement AcceptDeleteApplicationButton => Browser.WebDriver.FindElement(By.Id("confirmDeleteApplicationBtn"));

        public IWebElement AcceptDeleteEnvironmentButton => Browser.WebDriver.FindElement(By.Id("confirmDeleteEnvironmentBtn"));

        public IWebElement SelectedApplication => Browser.WebDriver.FindElement(By.Id("selectedApp"));

        public IWebElement EditEnvironmentNameInput => Browser.WebDriver.FindElement(By.Id("editEnvironmentNameInput"));

        public IWebElement SaveEnvironmentChangesButton => Browser.WebDriver.FindElement(By.Id("saveEditEnvironmentBtn"));

        public IWebElement DeleteEnvironmentButton => Browser.WebDriver.FindElement(By.Id("deleteEnvironmentBtn"));

        public IWebElement CancelEditFeatureFlagsModalButton => Browser.WebDriver.FindElement(By.Id("cancelEditToggleBtn"));

        private readonly By _rowSelector = By.CssSelector(".vgt-responsive> table > tbody> tr");

        private readonly By _statusesDropdown =
            By.CssSelector(
                "body > div > div> div > div > div > div> div > div> table > thead > tr:nth-child(2) > th:nth-child(8) > div > select");

        private readonly By _noFeatureToggleDisplayedText =
            By.CssSelector("body > div > div> div > div > div > div> div > div> table > tbody > tr > td > div > div");

        private readonly By _deleteFeatureToggleIcon =
            By.CssSelector(
                "body > div > div> div > div > div > div> div > div> table > tbody > tr > td:nth-child(1) > span > a:nth-child(2) > i");

        private readonly By _editFeatureToggleIcon =
            By.CssSelector(
                "body > div > div> div > div > div > div> div > div> table > tbody > tr > td:nth-child(1) > span > a > i");

        private readonly By _isPermanentFlag =
            By.CssSelector(
                "body > div > div> div > div > div > div> div > div> table > tbody > tr > td:nth-child(2) > span > span.label.label-danger");

        private readonly By _editEnvironmentIcon =
            By.CssSelector(
                "body > div > div> div > div > div > div> div > div> table > thead > tr:nth-child(1) > th:nth-child(4) > a > i");

        private readonly By _devCheckbok =
            By.CssSelector("div:nth-child(2) > div.col-sm-1.margin-top-14 > div > div > input[type=checkbox]");

        private readonly By _qaCheckbok =
            By.CssSelector("div:nth-child(3) > div.col-sm-1.margin-top-14 > div > div > input[type=checkbox]");

        private readonly By _devLastUpdatedDate =
            By.CssSelector("div:nth-child(2) > .margin-top-8 > div:nth-child(1)");

        private readonly By _qaLastUpdatedDate =
            By.CssSelector("div:nth-child(3) > .margin-top-8 > div:nth-child(1)");

        private readonly By _refreshedEnvMessage =
            By.CssSelector("body > div.fade.alert.alert-success.alert-dismissible.in");

        public IWebElement SelectedApplicationName =>
            Browser.WebDriver.FindElement(
                By.CssSelector("#app-sel  div  div  div:nth-child(1)"));

        public void SelectASpecificApplication(string applicationName)
        {
            Thread.Sleep(1000);
            SelectApplication.Click();
            var applications = PageHelpers.GetDropdownList(ApplicationsDropdown, "li");
            var applicationsCount = applications.Count;
            for (var i = 0; i < applicationsCount; i++)
            {
                if (applications[i].Text != applicationName) continue;
                applications[i].Click();
                break;
            }
        }

        public void FilterByAcceptedByUser(string status)
        {
            var dropdown = Browser.WebDriver.FindElement(_statusesDropdown);
            var statuses = new SelectElement(dropdown);
            statuses.Options[1].Click();
        }

        public void AddFeatureToggle(string newFeatureToggleName)
        {
            ToolsButton.Click();
            var options = PageHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[1].Click();
            FeatureToggleNameInput.SendKeys(newFeatureToggleName);
            NotesInput.SendKeys("test");
            AddFeatureToggleButton.Click();
            Thread.Sleep(1000);
            CloseAddToggleModalBtn.Click();
        }

        public void AddNewApplication(string newApplicationName, string firstEnvName)
        {
            ToolsButton.Click();
            var options = PageHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[2].Click();
            ApplicationNameInput.SendKeys(newApplicationName);
            FirstEnvNameInput.SendKeys(firstEnvName);
            AddApplicationButton.Click();
            Thread.Sleep(1000);
            CloseAddApplicationModalBtn.Click();
        }

        public void AddNewEnvironment(string newEnvironmentName)
        {
            ToolsButton.Click();
            var options = PageHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[3].Click();
            SecondEnvNameInput.SendKeys(newEnvironmentName);
            AddEnvironmentButton.Click();
            Thread.Sleep(1000);
            CloseAddEnvironmentModalBtn.Click();
        }

        public bool IsGridEmpty()
        {
            Thread.Sleep(1000);
            return PageHelpers.IsElementPresent(_noFeatureToggleDisplayedText);
        }

        public bool NewAddedFeatureToggleIsVisible(string newFeatureToggleName)
        {
            Thread.Sleep(1000);
            var rows = Utils.GetAllRowsFromGrid(FeatureTogglesGrid, _rowSelector);
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells[0].Text.Equals(newFeatureToggleName) || cells[1].Text.Equals(newFeatureToggleName))
                    return true;
            }

            return false;
        }

        public bool CreationDateIsCorrectlyDisplayed(string newFeatureToggleName)
        {
            var rows = Utils.GetAllRowsFromGrid(FeatureTogglesGrid, _rowSelector);
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (!cells[1].Text.Equals(newFeatureToggleName)) continue;
                var creationDateAndTime = cells[8].Text;
                var creationDate =
                    creationDateAndTime.Substring(0, creationDateAndTime.IndexOf(" ", StringComparison.Ordinal));
                var formattedCreationDate = DateTime.Parse(creationDate).Date;
                if (formattedCreationDate == DateTime.Now.Date)
                    return true;
            }

            return false;
        }

        public void DeleteFeatureToggle(string newFeatureToggleName)
        {
            Thread.Sleep(1000);
            var rows = Utils.GetAllRowsFromGrid(FeatureTogglesGrid, _rowSelector);
            for (var i = 0; i <= rows.Count-1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (!cells[1].Text.Contains(newFeatureToggleName)) continue;
                Utils.GetColumnSpecifiedByIndex(FeatureTogglesGrid, _rowSelector, i, 0).FindElement(_deleteFeatureToggleIcon)
                    .Click();
                DeleteFeatureToggleButton.Click();
            }
        }

        public void EditFeatureToggle(string newFeatureToggleName)
        {
            Thread.Sleep(1000);
            var rows = Utils.GetAllRowsFromGrid(FeatureTogglesGrid, _rowSelector);
            for (var i = 0; i <= rows.Count - 1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (cells[1].Text.Contains(newFeatureToggleName))
                {
                    Utils.GetColumnSpecifiedByIndex(FeatureTogglesGrid, _rowSelector, i, 0).FindElement(_editFeatureToggleIcon)
                        .Click();
                }
            }
        }

        public void SetFeatureToggleAsPermanent()
        {
            Thread.Sleep(1000);
            IsPermanentCheckbox.Click();
            SaveButton.Click();
        }

        public void SetFeatureToggleAsAcceptedByUser()
        {
            Thread.Sleep(1000);
            IsAcceptedByUserCheckbox.Click();
            SaveButton.Click();
        }

        public bool FeatureToggleIsPermanent()
        {
            Thread.Sleep(1000);
            FilterByACriteria.SendKeys(Constants.FeatureToggleName);
            return PageHelpers.IsElementPresent(_isPermanentFlag);
        }

        public void ChangeApplicationName(string currentApplicationName,string editedApplicationName)
        {
            var applicationName = GetSelectedApplicationName();
            if (currentApplicationName != applicationName) return;
            EditApplicationIcon.Click();
            
            EditApplicationNameInput.Clear();
         
            EditApplicationNameInput.SendKeys(editedApplicationName);
          
            SaveApplicationChangesButton.Click();
          
        }

        public bool ApplicationIsSelected(string applicationName)
        {
            return SelectedApplication.Text == applicationName;
        }

        public string GetSelectedApplicationName()
        {
            return SelectedApplicationName.Text;
        }

        public void DeleteApplication(string expectedApplicationName)
        {
            Thread.Sleep(1000);
            var applicationName = GetSelectedApplicationName();
            if (expectedApplicationName != applicationName) return;
            EditApplicationIcon.Click();
            DeleteApplicationButton.Click();
            AcceptDeleteApplicationButton.Click();
        }

        public bool ApplicationNameExists(string applicationName)
        {
            Thread.Sleep(1000);
            SelectApplication.Click();
            var applications = PageHelpers.GetDropdownList(ApplicationsDropdown, "li");
            var applicationsCount = applications.Count;
            for (var i = 0; i < applicationsCount; i++)
            {
                if (applications[i].Text.Equals(applicationName))
                    return true;
            }

            return false;
        }

        public void EditEnvironment(string environmentName)
        {
            var element = Utils.GetHeaderSpecifiedByIndex(FeatureTogglesGrid,3);
            Thread.Sleep(1000);
            if (element.Text.Equals(environmentName))
                {
                 element.FindElement(_editEnvironmentIcon).Click();
                }           
        }

        public void ChangeEnvironmentName(string editedEnvName)
        {
            EditEnvironmentNameInput.Clear();
            EditEnvironmentNameInput.SendKeys(editedEnvName);
            SaveEnvironmentChangesButton.Click();
        }

        public bool EnvironmentNameExist(string envName)
        {
            Thread.Sleep(1000);
            return Utils.GetHeaderSpecifiedByIndex(FeatureTogglesGrid,3).Text.Equals(envName);
        }

        public void DeleteEnvironment(string editedEnvName)
        {
            Thread.Sleep(1000);
            EditEnvironment(editedEnvName);
            DeleteEnvironmentButton.Click();
            Thread.Sleep(1000);
            AcceptDeleteEnvironmentButton.Click();
        }

        public bool IsDevEnvironmentCheckboxChecked()
        {         
            return PageHelpers.IsElementSelected(_devCheckbok);
        }

        public bool IsQaEnvironmentCheckboxChecked()
        {
            return PageHelpers.IsElementSelected(_qaCheckbok);
        }

        public bool IsLastUpdatedDateOnDevCorrectlyDisplayed()
        {
            return Utils.IsLastUpdatedDateCorrectlyDisplayed(_devLastUpdatedDate);
        }

        public bool IsLastUpdatedDateOnQaCorrectlyDisplayed()
        {
            return Utils.IsLastUpdatedDateCorrectlyDisplayed(_qaLastUpdatedDate);
        }

        public void CloseEditFeatureFlagsModal()
        {
            Thread.Sleep(1000);
            CancelEditFeatureFlagsModalButton.Click();
        }

        public void UpdateDevEnvironment()
        {
            Thread.Sleep(1000);
            DevEnvironmentCheckbox.Click();
            SaveButton.Click();     
        }

        public void RefreshEnvironment()
        {
            Thread.Sleep(1000);
            RefreshEnvironmentButton.Click();
        }

        public bool IsRefreshedEnvironmentMessageIsDisplayed()
        {
            WaitHelpers.WaitUntilElementIsVisible(_refreshedEnvMessage);
            return PageHelpers.IsElementEnabled(_refreshedEnvMessage);
        }
             
    }
}
