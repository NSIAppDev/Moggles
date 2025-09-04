using Microsoft.Identity.Client;
using Microsoft.Playwright;
using Moggles.E2EPlaywrightTests.Helpers;
using NSTestFrameworkDotNetCoreApi.RestSharp;

namespace Moggles.E2EPlaywrightTests.Pages
{
    public class FeatureTogglesPage(IPage featureTogglesPage)
    {
        private readonly IPage _featureTogglesPage = featureTogglesPage;
        protected readonly Utils Utils = new(featureTogglesPage);

        #region selectors
        private ILocator _toolsMenuDropdown => _featureTogglesPage.Locator(".dropdown-menu li");
        private ILocator _statusesDropdown => _featureTogglesPage.Locator("tr:nth-child(2) > th:nth-child(6) > div > select");
        private ILocator _openAddApplicationModalBtn => _featureTogglesPage.Locator("#showAddApplicationModalBtn");
        private ILocator _openDeletedFeatureTogglesSection => _featureTogglesPage.Locator("a > h4");
        private ILocator _addFeatureToggleButton => _featureTogglesPage.Locator("#addFeatureToggleBtn");
        private ILocator _closeAddToggleModalBtn => _featureTogglesPage.Locator("#closeAddToggleModalBtn");
        private ILocator _addApplicationButton => _featureTogglesPage.Locator("#addApplicationBtn");
        private ILocator _addEnvironmentButton => _featureTogglesPage.Locator("#addEnvironmentBtn");
        private ILocator _closeAddEnvironmentModalBtn => _featureTogglesPage.Locator("#closeAddEnvironmentModalBtn");

        private ILocator _featureToggleNameInput => _featureTogglesPage.Locator("#featureToggleName");
        private ILocator _notesInput => _featureTogglesPage.Locator("#notesInput");
        private ILocator _applicationNameInput => _featureTogglesPage.Locator("#addApplicationNameInput");
        private ILocator _firstEnvNameInput => _featureTogglesPage.Locator("#addFirstEnvironmentInput");
        private ILocator _environmentNameInput => _featureTogglesPage.Locator("#addEnvironmentNameInput");

        private ILocator _isPermanentCheckbox => _featureTogglesPage.Locator("#editIsPermanentCheckbox");
        private ILocator _isAcceptedByUserCheckbox => _featureTogglesPage.Locator("#editAcceptedByUserCheckbox");
        private ILocator _saveButton => _featureTogglesPage.Locator("#saveEditToggleBtn");

        private ILocator _editFeatureToggleIcon => _featureTogglesPage.Locator("#toggleGrid span > a:nth-child(1) > i");
        private ILocator _deleteFeatureToggleButtonOnEdit => _featureTogglesPage.Locator("#deleteToggleBtnEditModal");
        private ILocator _deleteFeatureToggleButton => _featureTogglesPage.Locator("#deleteToggleBtn");
        private ILocator _deleteFeatureToggleReason => _featureTogglesPage.Locator("body > div.modal.fade.in .modal-body .form-horizontal textarea");

        private ILocator _editApplicationIcon => _featureTogglesPage.Locator("#showEditApplicationModalBtn");
        private ILocator _editApplicationNameInput => _featureTogglesPage.Locator("#editApplicationNameInput");
        private ILocator _saveApplicationChangesButton => _featureTogglesPage.Locator("#saveEditApplicationBtn");
        private ILocator _confirmDeleteApplicationButton => _featureTogglesPage.Locator("#confirmDeleteApplicationBtn");
        private ILocator _deleteApplicationButton => _featureTogglesPage.Locator("#deleteApplicationBtn");

        private ILocator _editEnvironmentIcon => _featureTogglesPage.Locator("#toggleGrid tr:nth-child(1) > th:nth-child(4) > a > i");
        private ILocator _editEnvironmentNameInput => _featureTogglesPage.Locator("#editEnvironmentNameInput");
        private ILocator _confirmDeleteEnvironmentButton => _featureTogglesPage.Locator("#confirmDeleteEnvironmentBtn");
        private ILocator _saveEnvironmentChangesButton => _featureTogglesPage.Locator("#saveEditEnvironmentBtn");
        private ILocator _deleteEnvironmentButton => _featureTogglesPage.Locator("#deleteEnvironmentBtn");

        private ILocator _filterByACriteria => _featureTogglesPage.Locator("tr:nth-child(2) > th:nth-child(2) > div > input");
        private ILocator _refreshEnvironmentButton => _featureTogglesPage.Locator("#refreshEnvironmentsBtn");
        private ILocator _rowSelector => _featureTogglesPage.Locator(".vgt-responsive> table > tbody> tr");
        private ILocator _noFeatureToggleDisplayedText => _featureTogglesPage.Locator("#toggleGrid tr td div div");
        private ILocator _deleteFeatureToggleIcon => _featureTogglesPage.Locator("#toggleGrid span > a:nth-child(2) > i");
        private ILocator _isPermanentFlag => _featureTogglesPage.Locator(".label-danger");
        private ILocator _devCheckbox => _featureTogglesPage.Locator(".form-horizontal > div > div:nth-child(4) > div > div > div > input[type=checkbox]");
        private ILocator _qaCheckbox => _featureTogglesPage.Locator(".form-horizontal > div > div:nth-child(5) > div > div > div > input[type=checkbox]");
        private ILocator _devLastUpdatedDate => _featureTogglesPage.Locator("div:nth-child(4) > .col-sm-8 > div:nth-child(1)");
        private ILocator _qaLastUpdatedDate => _featureTogglesPage.Locator("div:nth-child(5) > .col-sm-8 > div:nth-child(1)");
        private ILocator _refreshedEnvMessage => _featureTogglesPage.Locator("body > div.fade.alert.alert-success.alert-dismissible.in");
        private ILocator _selectedApplication => _featureTogglesPage.Locator("#selectedApp");
        private ILocator _toolsButton => _featureTogglesPage.Locator("li.dropdown");
        private ILocator _selectedAppName => _featureTogglesPage.Locator("#app-sel  div  div  div:nth-child(1)");
        private ILocator _pageSpinner => _featureTogglesPage.Locator(".fa-spinner");
        private ILocator _applicationsList => _featureTogglesPage.Locator("body > ul > li > a");

        private ILocator _deletedFeatureTogglesPanel => _featureTogglesPage.Locator(".panel-body.padding-0");
        private ILocator _deletedFeatureToggleName => _featureTogglesPage.Locator("#deletedTogglesGrid tbody > tr:nth-child(1) > td:nth-child(2)");
        private ILocator _deleteAllDeletedFeatureTogglesCheckbox => _featureTogglesPage.Locator("#deletedTogglesGrid table > thead > tr:nth-child(1) > th > input[type=checkbox]");
        private ILocator _removeDeletedFeatureTogglesButton => _featureTogglesPage.Locator(".vgt-selection-info-row div > div > button");

        public ILocator FeatureTogglesGrid => _featureTogglesPage.Locator("#toggleGrid");
        #endregion
        public async Task<bool> IsGridEmpty()
        {
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
            return await _noFeatureToggleDisplayedText.IsVisibleAsync();
        }
        public async Task<bool> IsDevEnvironmentCheckboxChecked() => await _devCheckbox.IsCheckedAsync();
        public async Task<bool> IsQaEnvironmentCheckboxChecked() => await _qaCheckbox.IsCheckedAsync();
        public async Task<bool> IsRefreshedEnvironmentMessageIsDisplayed() => await _refreshedEnvMessage.IsEnabledAsync();
        public async Task<string> GetSelectedApplicationName() => await _selectedAppName.InnerTextAsync();
        public async Task SelectApplicationByName(string applicationName)
        {
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden});
            await _selectedApplication.ClickAsync();
            await Utils.SelectOptionFromListAsync(_applicationsList, applicationName);
        }
        public async Task FilterAcceptedByUserColumn(string status)
        {
            try
            {
                await _statusesDropdown.WaitForAsync();
                await Utils.SelectOptionFromListAsync(_statusesDropdown, status);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception in selecting element from User accepted drop-down: {ex.Message}");
            }
        }
        public async Task AddFeatureToggle(string newFeatureToggleName)
        {
            Thread.Sleep(2000);
            await Utils.SelectFromDropdownAsync(_toolsButton, _toolsMenuDropdown ,"Add Feature Toggle");
            await _featureToggleNameInput.WaitForAsync();
            await _featureToggleNameInput.FillAsync(newFeatureToggleName);
            await _notesInput.FillAsync("test notes");
            await _addFeatureToggleButton.ClickAsync();
            await _closeAddToggleModalBtn.ClickAsync();
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }
        public async Task AddNewApplication(string newApplicationName, string firstEnvName)
        {
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
            Thread.Sleep(2000);
            await _openAddApplicationModalBtn.WaitForAsync();
            await _openAddApplicationModalBtn.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
            await _applicationNameInput.FillAsync(newApplicationName);
            await _firstEnvNameInput.FillAsync(firstEnvName);
            await _addApplicationButton.ClickAsync();
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }
        public async Task AddNewEnvironment(string newEnvironmentName)
        {
            Thread.Sleep(2000);
            await _toolsButton.SelectOptionAsync("Add New Environment");
            await _environmentNameInput.FillAsync(newEnvironmentName);
            await _addEnvironmentButton.ClickAsync();
            await _closeAddEnvironmentModalBtn.ClickAsync();
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });
        }
        public async Task<bool> IsFeatureToggleDisplayed(string newFeatureToggleName)
        {
            // Wait for spinner to disappear
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });

            // Locate all rows inside the grid
            var rows = FeatureTogglesGrid.Locator(_rowSelector);
            int rowCount = await rows.CountAsync();

            for (int i = 0; i < rowCount; i++)
            {
                var row = rows.Nth(i);

                // Get all cells in the current row
                var cells = row.Locator("td");
                int cellCount = await cells.CountAsync();

                for (int j = 0; j < cellCount; j++)
                {
                    var text = await cells.Nth(j).InnerTextAsync();
                    if (text.Equals(newFeatureToggleName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
        public async Task<bool> IsCreationDateCorrectlyDisplayed(string newFeatureToggleName)
        {
            // Wait for spinner to disappear
            await _pageSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden });

            var rows = FeatureTogglesGrid.Locator(_rowSelector);
            int rowCount = await rows.CountAsync();

            for (int i = 0; i < rowCount; i++)
            {
                var row = rows.Nth(i);
                var cells = row.Locator("td");

                // Check if the name matches
                string nameCellText = await cells.Nth(1).InnerTextAsync();
                if (!nameCellText.Equals(newFeatureToggleName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extract creation date
                string creationDateAndTime = await cells.Nth(7).InnerTextAsync();
                string creationDate = creationDateAndTime.Split(' ')[0];

                DateTime parsedDate;
                if (DateTime.TryParse(creationDate, out parsedDate) && parsedDate.Date == DateTime.Now.Date)
                    return true;
            }

            return false;
        }
        public async Task DeleteFeatureToggle(string newFeatureToggleName, string reasonToDelete)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            var row = FeatureTogglesGrid
                .Locator("tr")
                .Filter(new LocatorFilterOptions { HasText = newFeatureToggleName })
                .First;

            if (await row.CountAsync() == 0) return; // not found

            await row.Locator(_deleteFeatureToggleIcon).ClickAsync();
            await _deleteFeatureToggleReason.FillAsync(reasonToDelete);
            await _deleteFeatureToggleButton.Nth(1).ClickAsync();

            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
        public async Task EditFeatureToggle(string newFeatureToggleName)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            var row = FeatureTogglesGrid
                .Locator("tr")
                .Filter(new LocatorFilterOptions { HasText = newFeatureToggleName })
                .First;

            if (await row.CountAsync() == 0) return; // not found

            await row.Locator("span > a:nth-child(1) > i").ClickAsync();
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
        public async Task SetFeatureToggleAsPermanent()
        {
            await _isPermanentCheckbox.ClickAsync();
            await _saveButton.ClickAsync();
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
        public async Task SetFeatureToggleAsAcceptedByUser()
        {
            await _isAcceptedByUserCheckbox.ClickAsync();
            await _saveButton.ClickAsync();
        }

        public async Task<bool> IsFeatureTogglePermanent()
        {
            await _filterByACriteria.FillAsync(Constants.FeatureToggleName);
            return await _isPermanentFlag.IsVisibleAsync();
        }

        public async Task ChangeApplicationName(string currentApplicationName, string editedApplicationName)
        {
            if (currentApplicationName != await GetSelectedApplicationName()) return;
            await _editApplicationIcon.ClickAsync();
            await _editApplicationNameInput.ClearAsync();
            await _editApplicationNameInput.FillAsync(editedApplicationName);
            await _saveApplicationChangesButton.ClickAsync();
        }
        public async Task DeleteApplication(string expectedApplicationName)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            if (expectedApplicationName != await GetSelectedApplicationName()) return;
            await _editApplicationIcon.ClickAsync();
            await _deleteApplicationButton.ClickAsync();
            await _confirmDeleteApplicationButton.ClickAsync();
        }
        public async Task<bool> IsApplicationListed(string applicationName)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            var dropdownElements = _featureTogglesPage.Locator("#selectedApp ul li");
            foreach (var element in await dropdownElements.AllAsync())
            {
                var text = await element.InnerTextAsync();
                if (text.Equals(applicationName))
                    return true;
            }
            return false;
        }

        public async Task EditEnvironment(string environmentName)
        {
            var element = await Utils.GetHeaderSpecifiedByIndexAsync("#FeatureTogglesGrid", 3);
            var text = await element.InnerTextAsync();
            if (text.Equals(environmentName))
            {
                await element.Locator(_editEnvironmentIcon).First.ClickAsync();
            }
        }

        public async Task ChangeEnvironmentName(string editedEnvName)
        {
            await _editEnvironmentNameInput.ClearAsync();
            await _editEnvironmentNameInput.FillAsync(editedEnvName);
            await _saveEnvironmentChangesButton.ClickAsync();
        }

        async Task<bool> IsEnvironmentNameDisplayed(string envName)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            var element = await Utils.GetHeaderSpecifiedByIndexAsync("#FeatureTogglesGrid", 3);

            return element.InnerTextAsync().Equals(envName);
        }

        public async Task DeleteEnvironment(string editedEnvName)
        {
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            await EditEnvironment(editedEnvName);
            await _deleteEnvironmentButton.ClickAsync();
            await _confirmDeleteEnvironmentButton.ClickAsync();
        }

        async Task<bool> IsLastUpdatedDateOnDevCorrectlyDisplayed() => await Utils.IsLastUpdatedDateCorrectlyDisplayedAsync(_devLastUpdatedDate);

        async Task<bool> IsLastUpdatedDateOnQaCorrectlyDisplayed() => await Utils.IsLastUpdatedDateCorrectlyDisplayedAsync(_qaLastUpdatedDate);

        public async Task UpdateDevEnvironment()
        {
            await _deleteFeatureToggleButtonOnEdit.WaitForAsync();
            await _devCheckbox.ClickAsync();
            await _saveButton.ClickAsync();
        }

        public async Task RefreshEnvironment() => await _refreshEnvironmentButton.ClickAsync();

        public async Task DeleteToggleOnEdit(string reasonToDelete)
        {
            // Wait for spinner to disappear
            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            // Wait until the delete button on edit is visible and click
            await _deleteFeatureToggleButtonOnEdit.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await _deleteFeatureToggleButtonOnEdit.ClickAsync();

            // Find the third element for the reason input (index 2)
            var reasonInput = _deleteFeatureToggleReason.Nth(2);
            await reasonInput.ClickAsync();
            await reasonInput.FillAsync(reasonToDelete);

            // Click the second delete button (index 1)
            var deleteButton = _deleteFeatureToggleButton.Nth(1);
            await deleteButton.ClickAsync();

            await _pageSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }


        public async Task OpenDeletedFeatureTogglesSection() => await _openDeletedFeatureTogglesSection.ClickAsync();

        async Task<bool> IsDeletedFeatureTogglesPanelVisible() => await _deletedFeatureTogglesPanel.IsVisibleAsync();

        async Task<string> GetDeletedFeatureToggleNameFromGrid()
        {
            await _deletedFeatureTogglesPanel.WaitForAsync();
            return await _deletedFeatureToggleName.InnerTextAsync();
        }

        public async Task RemoveAllDeletedFeatureToggles()
        {
            await _deleteAllDeletedFeatureTogglesCheckbox.ClickAsync();
            await _removeDeletedFeatureTogglesButton.ClickAsync();
        }

    }
}
