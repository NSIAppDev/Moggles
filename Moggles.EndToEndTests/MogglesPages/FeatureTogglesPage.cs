﻿using System;
using System.Linq;
using System.Threading;
using Moggles.EndToEndTests.TestFramework;
using NsTestFrameworkUI.Helpers;
using NsTestFrameworkUI.KendoHelpers;
using NsTestFrameworkUI.Pages;
using OpenQA.Selenium;

namespace Moggles.EndToEndTests.MogglesPages
{
    public class FeatureTogglesPage
    {
        #region selectors
        private readonly By _toolsMenuDropdown = By.CssSelector(".dropdown-menu li");
        private readonly By _statusesDropdown = By.CssSelector("tr:nth-child(2) > th:nth-child(8) > div > select");
        private readonly By _openAddApplicationModalBtn = By.Id("showAddApplicationModalBtn");

        private readonly By _addFeatureToggleButton = By.Id("addFeatureToggleBtn");
        private readonly By _closeAddToggleModalBtn = By.Id("closeAddToggleModalBtn");
        private readonly By _addApplicationButton = By.Id("addApplicationBtn");
        private readonly By _addEnvironmentButton = By.Id("addEnvironmentBtn");
        private readonly By _closeAddEnvironmentModalBtn = By.Id("closeAddEnvironmentModalBtn");

        private readonly By _featureToggleNameInput = By.Id("featureToggleName");
        private readonly By _notesInput = By.Id("notesInput");
        private readonly By _applicationNameInput = By.Id("addApplicationNameInput");
        private readonly By _firstEnvNameInput = By.Id("addFirstEnvironmentInput");
        private readonly By _environmentNameInput = By.Id("addEnvironmentNameInput");

        private readonly By _isPermanentCheckbox = By.Id("editIsPermanentCheckbox");
        private readonly By _isAcceptedByUserCheckbox = By.Id("editAcceptedByUserCheckbox");
        private readonly By _saveButton = By.Id("saveEditToggleBtn");

        private readonly By _editFeatureToggleIcon = By.CssSelector("#toggleGrid span > a:nth-child(1) > i");
        private readonly By _deleteFeatureToggleButtonOnEdit = By.Id("deleteToggleBtnEditModal");
        private readonly By _deleteFeatureToggleButton = By.Id("deleteToggleBtn");
        private readonly By _deleteFeatureToggleReason = By.CssSelector("body > div.modal.fade.in .modal-body .form-horizontal textarea");

        private readonly By _editApplicationIcon = By.Id("showEditApplicationModalBtn");
        private readonly By _editApplicationNameInput = By.Id("editApplicationNameInput");
        private readonly By _saveApplicationChangesButton = By.Id("saveEditApplicationBtn");
        private readonly By _confirmDeleteApplicationButton = By.Id("confirmDeleteApplicationBtn");
        private readonly By _deleteApplicationButton = By.Id("deleteApplicationBtn");

        private readonly By _editEnvironmentIcon = By.CssSelector("#toggleGrid tr:nth-child(1) > th:nth-child(4) > a > i");
        private readonly By _editEnvironmentNameInput = By.Id("editEnvironmentNameInput");
        private readonly By _confirmDeleteEnvironmentButton = By.Id("confirmDeleteEnvironmentBtn");
        private readonly By _saveEnvironmentChangesButton = By.Id("saveEditEnvironmentBtn");
        private readonly By _deleteEnvironmentButton = By.Id("deleteEnvironmentBtn");

        private readonly By _filterByACriteria = By.CssSelector("tr:nth-child(2) > th:nth-child(2) > div > input");
        private readonly By _refreshEnvironmentButton = By.Id("refreshEnvironmentsBtn");
        private readonly By _rowSelector = By.CssSelector(".vgt-responsive> table > tbody> tr");
        private readonly By _noFeatureToggleDisplayedText = By.CssSelector("#toggleGrid tr td div div");
        private readonly By _deleteFeatureToggleIcon = By.CssSelector("#toggleGrid span > a:nth-child(2) > i");
        private readonly By _isPermanentFlag = By.CssSelector(".label-danger");
        private readonly By _devCheckbox =
            By.CssSelector("div:nth-child(4) > div > div > div > input[type=checkbox]");
        private readonly By _qaCheckbox =
            By.CssSelector("div:nth-child(5) > div > div > div > input[type=checkbox]");
        private readonly By _devLastUpdatedDate =
            By.CssSelector("div:nth-child(4) > .col-sm-8 > div:nth-child(1)");
        private readonly By _qaLastUpdatedDate =
            By.CssSelector("div:nth-child(5) > .col-sm-8 > div:nth-child(1)");
        private readonly By _refreshedEnvMessage =
            By.CssSelector("body > div.fade.alert.alert-success.alert-dismissible.in");
        private readonly By _selectedApplication = By.Id("selectedApp");
        private readonly By _toolsButton = By.CssSelector("li.dropdown");
        private readonly By _selectedAppName = By.CssSelector("#app-sel  div  div  div:nth-child(1)");
        private readonly By _pageSpinner = By.CssSelector(".fa-spinner");
        private readonly By _applicationsList = By.CssSelector("body > ul > li > a");


        public IWebElement FeatureTogglesGrid => Browser.WebDriver.FindElement(By.Id("toggleGrid"));
        #endregion

        public void Navigate()
        {
            Browser.GoTo(Constants.BaseUrl);
            if (!_addApplicationButton.IsElementPresent())
                Browser.GoTo(Constants.BaseUrl);
        }

        public bool IsGridEmpty() => _noFeatureToggleDisplayedText.IsElementPresent();
        public bool IsDevEnvironmentCheckboxChecked()
        {
            WaitHelpers.ExplicitWait();
            return _devCheckbox.IsElementSelected();
        }
        public bool IsQaEnvironmentCheckboxChecked()
        {
            WaitHelpers.ExplicitWait();
            return _qaCheckbox.IsElementSelected();
        }
        public bool IsRefreshedEnvironmentMessageIsDisplayed()
        {
            WaitHelpers.ExplicitWait();
            return _refreshedEnvMessage.IsElementEnabled();
        }

        public string GetSelectedApplicationName()
        {
            WaitHelpers.ExplicitWait();
            return _selectedAppName.GetText();
        }

        public void SelectApplicationByName(string applicationName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            _selectedApplication.ActionClick();
            _applicationsList.SelectOptionFromList(applicationName);
        }

        public void FilterAcceptedByUserColumn(string status)
        {
            _statusesDropdown.WaitForElementToBeClickable();
            _statusesDropdown.SelectFromDropdownByText(status);
        }

        public void AddFeatureToggle(string newFeatureToggleName)
        {
            WaitHelpers.ExplicitWait();
            Thread.Sleep(2000);
            _toolsButton.SelectFromDropdown(_toolsMenuDropdown, "Add Feature Toggle");
            _featureToggleNameInput.ActionSendKeys(newFeatureToggleName);
            //_workItemIdInput.ActionSendKeys("TESTING PBI 00000");
            _notesInput.ActionSendKeys("test notes");
            _addFeatureToggleButton.ActionClick();
            WaitHelpers.ExplicitWait();
            _closeAddToggleModalBtn.ActionClick();
            _pageSpinner.WaitForSpinner();
        }

        public void AddNewApplication(string newApplicationName, string firstEnvName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            Thread.Sleep(2000);
            _openAddApplicationModalBtn.WaitForElementToBeClickable();
            _openAddApplicationModalBtn.ActionClick();
            _applicationNameInput.ActionSendKeys(newApplicationName);
            _firstEnvNameInput.ActionSendKeys(firstEnvName);
            _addApplicationButton.ActionClick();
            _pageSpinner.WaitForSpinner();
        }

        public void AddNewEnvironment(string newEnvironmentName)
        {
            WaitHelpers.ExplicitWait();
            Thread.Sleep(2000);
            _toolsButton.SelectFromDropdown(_toolsMenuDropdown, "Add New Environment");
            _environmentNameInput.ActionSendKeys(newEnvironmentName);
            _addEnvironmentButton.ActionClick();
            WaitHelpers.ExplicitWait();
            _closeAddEnvironmentModalBtn.ActionClick();
            _pageSpinner.WaitForSpinner();
        }

        public bool IsFeatureToggleDisplayed(string newFeatureToggleName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
            foreach (var row in rows)
            {
                var cells = row.FindElements(By.TagName("td"));
                if (cells[0].Text.Equals(newFeatureToggleName) || cells[1].Text.Equals(newFeatureToggleName))
                    return true;
            }
            return false;
        }

        public bool IsCreationDateCorrectlyDisplayed(string newFeatureToggleName)
        {
            _pageSpinner.WaitForSpinner();
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

        public void DeleteFeatureToggle(string newFeatureToggleName, string reasonToDelete)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
            for (var i = 0; i <= rows.Count - 1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (!cells[1].Text.Contains(newFeatureToggleName)) continue;
                WaitHelpers.ExplicitWait();
                _deleteFeatureToggleIcon.WaitUntilElementIsVisible();
                FeatureTogglesGrid.GetColumnSpecifiedByIndex(_rowSelector, i, 0).FindElement(_deleteFeatureToggleIcon)
                    .Click();
                WaitHelpers.ExplicitWait();
                _deleteFeatureToggleReason.ActionClick();
                _deleteFeatureToggleReason.ActionSendKeys(reasonToDelete);
                _deleteFeatureToggleButton.ActionClick();
                _pageSpinner.WaitForSpinner();
            }
        }

        public void EditFeatureToggle(string newFeatureToggleName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            Thread.Sleep(5000);
            var rows = FeatureTogglesGrid.GetAllRowsFromGrid(_rowSelector);
            for (var i = 0; i <= rows.Count - 1; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (cells[1].Text.Contains(newFeatureToggleName))
                {
                    FeatureTogglesGrid.GetColumnSpecifiedByIndex(_rowSelector, i, 0).FindElement(_editFeatureToggleIcon)
                        .Click();
                }
                WaitHelpers.ExplicitWait();
                _pageSpinner.WaitForSpinner();
            }
        }

        public void SetFeatureToggleAsPermanent()
        {
            WaitHelpers.ExplicitWait();
            _isPermanentCheckbox.ActionClick();
            _saveButton.ActionClick();
            _pageSpinner.WaitForSpinner();
        }

        public void SetFeatureToggleAsAcceptedByUser()
        {
            _isAcceptedByUserCheckbox.ActionClick();
            _saveButton.ActionClick();
        }

        public bool IsFeatureTogglePermanent()
        {
            WaitHelpers.ExplicitWait();
            _filterByACriteria.ActionSendKeys(Constants.FeatureToggleName);
            return _isPermanentFlag.IsElementPresent();
        }

        public void ChangeApplicationName(string currentApplicationName, string editedApplicationName)
        {
            WaitHelpers.ExplicitWait();
            if (currentApplicationName != GetSelectedApplicationName()) return;
            _editApplicationIcon.ActionClick();
            _editApplicationNameInput.ClearField();
            _editApplicationNameInput.ActionSendKeys(editedApplicationName);
            _saveApplicationChangesButton.ActionClick();
            WaitHelpers.ExplicitWait();
        }

        public void DeleteApplication(string expectedApplicationName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            if (expectedApplicationName != GetSelectedApplicationName()) return;
            _editApplicationIcon.ActionClick();
            _deleteApplicationButton.ActionClick();
            _confirmDeleteApplicationButton.ActionClick();
            WaitHelpers.ExplicitWait();
        }

        public bool IsApplicationListed(string applicationName)
        {
            _pageSpinner.WaitForSpinner();
            var dropdownElements = Browser.WebDriver.FindElements(By.CssSelector("#selectedApp ul li"));
            return dropdownElements.Any(x => x.Text.Equals(applicationName));
        }

        public void EditEnvironment(string environmentName)
        {
            WaitHelpers.ExplicitWait();
            var element = FeatureTogglesGrid.GetHeaderSpecifiedByIndex(3);
            WaitHelpers.ExplicitWait();
            if (element.Text.Equals(environmentName))
            {
                element.FindElement(_editEnvironmentIcon).Click();
            }
        }

        public void ChangeEnvironmentName(string editedEnvName)
        {
            _editEnvironmentNameInput.ClearField();
            _editEnvironmentNameInput.ActionSendKeys(editedEnvName);
            _saveEnvironmentChangesButton.ActionClick();
        }

        public bool IsEnvironmentNameDisplayed(string envName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            return FeatureTogglesGrid.GetHeaderSpecifiedByIndex(3).Text.Equals(envName);
        }

        public void DeleteEnvironment(string editedEnvName)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            EditEnvironment(editedEnvName);
            _deleteEnvironmentButton.ActionClick();
            _confirmDeleteEnvironmentButton.ActionClick();
        }

        public bool IsLastUpdatedDateOnDevCorrectlyDisplayed()
        {
            return Utils.IsLastUpdatedDateCorrectlyDisplayed(_devLastUpdatedDate);
        }

        public bool IsLastUpdatedDateOnQaCorrectlyDisplayed()
        {
            return Utils.IsLastUpdatedDateCorrectlyDisplayed(_qaLastUpdatedDate);
        }

        public void UpdateDevEnvironment()
        {
            Browser.WebDriver.FindElement(_devCheckbox).Click();
            _saveButton.ActionClick();
        }

        public void RefreshEnvironment()
        {
            WaitHelpers.ExplicitWait();
            _refreshEnvironmentButton.ActionClick();
        }

        public void DeleteToggleOnEdit(string reasonToDelete)
        {
            _pageSpinner.WaitForSpinner();
            WaitHelpers.ExplicitWait();
            _deleteFeatureToggleButtonOnEdit.WaitUntilElementIsVisible();
            _deleteFeatureToggleButtonOnEdit.ActionClick();
            WaitHelpers.ExplicitWait();
            Browser.WebDriver.FindElements(_deleteFeatureToggleReason)[2].Click();
            Browser.WebDriver.FindElements(_deleteFeatureToggleReason)[2].SendKeys(reasonToDelete);
            Browser.WebDriver.FindElements(_deleteFeatureToggleButton)[1].Click();
        }

    }
}
