using MogglesEndToEndTests.TestFramework;
using NSTestFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using Browser = MogglesEndToEndTests.TestFramework.Browser;

namespace MogglesEndToEndTests.MogglesPages
{
    public class FeatureTogglesPage
    {
        public IWebElement ToolsMenuDropdown =>
            Browser.WebDriver.FindElement(By.CssSelector("#bs-example-navbar-collapse-1 > ul > li > ul"));

        public IWebElement ToolsButton =>
            Browser.WebDriver.FindElement(By.CssSelector("#bs-example-navbar-collapse-1 > ul > li > a"));

        public IWebElement FeatureToggleNameInput => Browser.WebDriver.FindElement(
            By.CssSelector("body > div > div.modal.in> div > div> div> div > div> div:nth-child(1)> input"));

        public IWebElement NotesInput => Browser.WebDriver.FindElement(
            By.CssSelector("body > div > div.modal.in> div > div> div> div > div> div:nth-child(2)> input"));

        public IWebElement AddButton => Browser.WebDriver.FindElement(
            By.CssSelector("body > div> div.in> div > div > div> div > div > div > button"));

        public IWebElement CloseModal =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div:nth-child(1) > div.modal.fade.in> div > div > div.modal-header > button"));

        public IWebElement FeatureTogglesGrid =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div > div> div > div > div > div> div > div.vgt-responsive"));

        public IWebElement DeleteFeatureToggleButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div:nth-child(1) > div> div > div > div > div.modal.fade.in> div > div > div> div > button.btn.btn-primary"));

        public IWebElement IsPermanentCheckbox =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div:nth-child(1) > div> div > div > div > div> div > div > div> div > div:nth-child(9) > div > div> div > div > input[type=checkbox]"));

        public IWebElement FilterByACriteria =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div > div> div > div > div > div> div > div> table > thead > tr:nth-child(2) > th:nth-child(2) > div > input"));

        public IWebElement IsAcceptedByUserCheckbox =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div:nth-child(1) > div> div > div > div > div> div > div > div> div > div:nth-child(10) > div > div> div > div > input[type=checkbox]"));

        public IWebElement SaveButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector(
                    "body > div:nth-child(1) > div> div > div > div > div.modal.fade.in> div > div > div> div > button.btn.btn-primary"));

        public IWebElement SelectApplication =>
            Browser.WebDriver.FindElement(By.CssSelector("#app-sel > div"));

        public IWebElement ApplicationNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div.in> div > div > div> div > div > div> input"));

        public IWebElement ApplicationsDropdown =>
            Browser.WebDriver.FindElement(By.CssSelector("#app-sel > div > ul"));

        public IWebElement EnvNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div.in > div > div > div> div > div > div> div > input"));

        public IWebElement EnvTrueToggle =>
            Browser.WebDriver.FindElement(By.Id("d1"));

        public IWebElement EditApplicationIcon =>
            Browser.WebDriver.FindElement(By.CssSelector("#bs-example-navbar-collapse-1 > ul> li > div > a > i"));

        public IWebElement EditApplicationNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div.in > div > div > div> div > div> div> div > input"));

        public IWebElement SaveApplicationChangesButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div.in > div > div > div> div > div> div > div> button.btn.btn-primary"));

        public IWebElement DeleteApplicationButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div.in > div > div > div> div > div> div > div:nth-child(2)> button"));

        public IWebElement AcceptDeleteApplicationButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div> div > div > div> div > div> button.btn.btn-primary"));

        public IWebElement AcceptDeleteEnvironmentButton =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div> div > div > div > div:nth-child(5)> div > div > div> div > .btn-primary"));

        public IWebElement SelectedApplication =>
            Browser.WebDriver.FindElement(By.CssSelector("#app-sel > div > div > div:nth-child(1)"));

        public IWebElement EditEnvironmentNameInput =>
            Browser.WebDriver.FindElement(
                By.CssSelector("body > div> div> div > div > div > div.in > div > div > div> div> div> div > input"));

        public IWebElement SaveEnvironmentChangesButton =>
            Browser.WebDriver.FindElement(By.CssSelector(
                "body > div> div> div > div > div > div.in > div > div > div> div> div> button.btn.btn-primary"));

        public IWebElement DeleteEnvironmentButton =>
            Browser.WebDriver.FindElement(By.CssSelector(
                "body > div> div> div > div > div > div.in > div > div > div> div> div:nth-child(1)> button"));

        private readonly By _rowSelector = By.CssSelector(".vgt-responsive>table>tbody>tr");

        private readonly By _headerSelector =
            By.CssSelector("body > div > div> div > div > div > div> div > div> table > thead > tr:nth-child(1)");

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

        public void Navigate()
        {
            Browser.Goto(Constants.BaseUrl);

            if (Browser.WebDriver.Title.Contains("Unauthorized"))
            {
                System.Diagnostics.Trace.WriteLine(
                    $"Authorization issue: {Browser.WebDriver.Url} - {Browser.WebDriver.Title}");
                throw new Exception();
            }

            IWait<IWebDriver> wait = new WebDriverWait(Browser.WebDriver, TimeSpan.FromSeconds(60));
            wait.Until(
                d => ((IJavaScriptExecutor) Browser.WebDriver).ExecuteScript("return document.readyState")
                    .Equals("complete"));

            Thread.Sleep(5000);
        }

        public void SelectASpecificApplication(string applicationName)
        {
            Thread.Sleep(1000);
            SelectApplication.Click();
            var applications = DropdownHelpers.GetDropdownList(ApplicationsDropdown, "li");
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
            var options = DropdownHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[1].Click();
            FeatureToggleNameInput.SendKeys(newFeatureToggleName);
            NotesInput.SendKeys("test");
            AddButton.Click();
            Thread.Sleep(1000);
            CloseModal.Click();
        }

        public void AddNewApplication(string newApplicationName, string firstEnvName)
        {
            ToolsButton.Click();
            var options = DropdownHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[2].Click();
            ApplicationNameInput.SendKeys(newApplicationName);
            EnvNameInput.SendKeys(firstEnvName);
            EnvTrueToggle.Click();
            AddButton.Click();
            Thread.Sleep(1000);
            CloseModal.Click();
        }

        public void AddNewEnvironment(string newEnvironmentName)
        {
            ToolsButton.Click();
            var options = DropdownHelpers.GetDropdownList(ToolsMenuDropdown, "li");
            Thread.Sleep(1000);
            options[3].Click();
            EnvNameInput.SendKeys(newEnvironmentName);
            AddButton.Click();
            Thread.Sleep(1000);
            CloseModal.Click();
        }

        public bool IsGridEmpty()
        {
            Thread.Sleep(1000);
            return Utils.IsElementDisplayedOnScreen(_noFeatureToggleDisplayedText);
        }

        public bool NewAddedFeatureToggleIsVisible(string newFeatureToggleName)
        {
            Thread.Sleep(1000);
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
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
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
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
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
            for (var i = 0; i <= rows.Count - 1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (!cells[1].Text.Contains(newFeatureToggleName)) continue;
                FeatureTogglesGrid.GetColumnSpecifiedByIndex(_rowSelector, i, 0).FindElement(_deleteFeatureToggleIcon)
                    .Click();
                DeleteFeatureToggleButton.Click();
            }
        }

        public void EditFeatureToggle(string newFeatureToggleName)
        {
            Thread.Sleep(1000);
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
            for (var i = 0; i <= rows.Count - 1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (cells[1].Text.Contains(newFeatureToggleName))
                {
                    FeatureTogglesGrid.GetColumnSpecifiedByIndex(_rowSelector, i, 0).FindElement(_editFeatureToggleIcon)
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
            return Utils.IsElementDisplayedOnScreen(_isPermanentFlag);
        }

        public void ChangeApplicationName(string editedApplicationName)
        {
            EditApplicationIcon.Click();
            EditApplicationNameInput.Clear();
            EditApplicationNameInput.SendKeys(editedApplicationName);
            SaveApplicationChangesButton.Click();
        }

        public bool ApplicationIsSelected(string applicationName)
        {
            return SelectedApplication.Text == applicationName;
        }

        public void DeleteApplication()
        {
            Thread.Sleep(1000);
            EditApplicationIcon.Click();
            DeleteApplicationButton.Click();
            AcceptDeleteApplicationButton.Click();
        }

        public bool ApplicationNameExists(string applicationName)
        {
            Thread.Sleep(1000);
            SelectApplication.Click();
            var applications = DropdownHelpers.GetDropdownList(ApplicationsDropdown, "li");
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
            Thread.Sleep(1000);
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_headerSelector);
            var cells = rows[0].FindElements(By.TagName("th"));
            if (cells[3].Text.Equals(environmentName))
                {
                 FeatureTogglesGrid.GetHeaderSpecifiedByIndex(_headerSelector, 0, 3)
                 .FindElement(_editEnvironmentIcon)
                 .Click();
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
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_headerSelector);
            var cells = rows[0].FindElements(By.TagName("th"));
            return cells[3].Text.Equals(envName);
        }

        public void DeleteEnvironment(string editedEnvName)
        {
            Thread.Sleep(1000);
            EditEnvironment(editedEnvName);
            DeleteEnvironmentButton.Click();
            Thread.Sleep(1000);
            AcceptDeleteEnvironmentButton.Click();
        }
    }
}
