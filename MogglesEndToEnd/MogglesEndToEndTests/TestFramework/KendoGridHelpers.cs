using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using NSTestFramework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace MogglesEndToEndTests.TestFramework
{
    public static class KendoGridHelpers
    {
        private static readonly string _dataColumnCssSelector = "[ng-bind],[ng-bind-custom]";

        public static List<KendoGridHeaderModel> GetGridHeaders(this IWebElement grid)
        {
            AssertIsKendoGrid(grid);

            var lockedHeaderElementsCount =
                grid.FindElements(By.CssSelector(".k-grid-header-locked table th[data-index]")).Count;

            var headerElements = grid.FindElements(By.CssSelector(".k-grid-header-wrap table th[data-index]"));
            var models = headerElements.Select(webElement =>
            {
                var dataField = webElement.GetAttribute("data-field");
                if (string.IsNullOrEmpty(dataField))
                {
                    webElement.Focus();
                    dataField = webElement.Text.ConvertToValidCSharpPropertyName();
                }

                return new KendoGridHeaderModel
                {
                    Index = GetColumnIndexFromHeader(webElement) - lockedHeaderElementsCount,
                    PropertyName = dataField,
                    Text = webElement.Text,
                    HeaderWebElement = webElement
                };
            }).OrderBy(_ => _.Index).ToList();
            return models;
        }


        public static TGridViewModel GetFirstKendoGridData<TGridViewModel>(this IWebElement grid,
            Expression<Func<TGridViewModel, bool>> condition = null)
            where TGridViewModel : class, new()
        {
            AssertIsKendoGrid(grid);
            var headers = grid.GetGridHeaders();
            return grid.GetKendoGridRow(condition).ParseRowData<TGridViewModel>(headers);
        }

        public static List<TGridViewModel> GetKendoGridDataUsingModels<TGridViewModel>(this IWebElement grid)
            where TGridViewModel : class, new()
        {
            AssertIsKendoGrid(grid);
            var headers = grid.GetGridHeaders();
            return grid
                .GetKendoGridRows()
                .Select(gr => gr.ParseRowData<TGridViewModel>(headers)).Where(m => m != null)
                .ToList();
        }

        public static IWebElement GetKendoGridRow<TGridViewModel>(this IWebElement grid,
            Expression<Func<TGridViewModel, bool>> condition)
            where TGridViewModel : class, new()
        {
            AssertIsKendoGrid(grid);
            var headers = grid.GetGridHeaders();
            return grid
                .GetKendoGridRows()
                .FirstOrDefault(r =>
                    condition == null || condition.Compile().Invoke(r.ParseRowData<TGridViewModel>(headers)));
        }


        public static bool KendoGridHasData<TGridViewModel>(this IWebElement grid,
            Expression<Func<TGridViewModel, bool>> condition)
            where TGridViewModel : class, new()
        {
            AssertIsKendoGrid(grid);

            return grid.GetKendoGridData<TGridViewModel>().Any(c => condition.Compile().Invoke(c));
        }

        private static TGridViewModel ParseColumns<TGridViewModel>(ReadOnlyCollection<IWebElement> columns,
            TGridViewModel dataItem, List<KendoGridHeaderModel> headers)
        {
            foreach (var header in headers)
            {
                if (HeaderIsEmpty(header))
                    continue;

                var columnWebElement = columns[header.Index];
                columnWebElement.Focus();
                var value = columnWebElement.Text;
                SetProperty(dataItem, header.PropertyName.Capitalize(), value);
            }

            return dataItem;
        }

        private static bool HeaderIsEmpty(KendoGridHeaderModel header)
        {
            return string.IsNullOrEmpty(header.PropertyName);
        }

        public static void KendoGridNavigateToNextPage(this IWebElement grid, Action onNavigationCompleted = null)
        {
            AssertIsKendoGrid(grid);

            var goToNextPageButton =
                grid.FindElements(By.CssSelector("a[title='Go to the next page']")).FirstOrDefault();

            if (NotFound(goToNextPageButton))
                throw new InvalidOperationException("Kendo Grid next page button not found");

            goToNextPageButton.Click();

            onNavigationCompleted?.Invoke();
        }

        public static void KendoGridNavigateToPreviousPage(this IWebElement grid, Action onNavigationCompleted = null)
        {
            AssertIsKendoGrid(grid);

            var goToPreviousPageButton =
                grid.FindElements(By.CssSelector("a[title='Go to the previous page']")).FirstOrDefault();

            if (NotFound(goToPreviousPageButton))
                throw new InvalidOperationException("Kendo Grid next page button not found");

            goToPreviousPageButton.Click();

            onNavigationCompleted?.Invoke();
        }

        public static void KendoGridShouldHavePageNumber(this IWebElement grid, int pageNumber)
        {
            AssertIsKendoGrid(grid);

            var gridId = grid.GetAttribute("id");
            var fallbackGridSelector = "[kendo-grid]";
            var gridSelector = !string.IsNullOrEmpty(gridId) ? $"#{gridId}" : fallbackGridSelector;

            WaitHelpers.WaitUntilElementTextEquals($"{gridSelector} .k-pager-numbers .k-state-selected",
                pageNumber.ToString());
        }

        public static void SetKendoGridInlineEditRowData(this IWebElement row, string property, string value,
            bool isJsonObject = false)
        {
            AssertIsKendoGridRow(row);

            var readOnlyControl = row
                .FindElements(By.CssSelector(_dataColumnCssSelector))
                .FirstOrDefault(c => GetPropertyName(c).Equals(property));

            if (IsValidDataColumn(readOnlyControl))
            {
                var camelCasePropertyName = $"{char.ToLower(property[0])}{property.Substring(1)}";

                IJavaScriptExecutor executor = (IJavaScriptExecutor)NSTestFramework.Browser.WebDriver;
                executor.ExecuteScript("arguments[0].click();", readOnlyControl);
                SetValue(row, value, camelCasePropertyName, isJsonObject);
            }
        }

        private static void SetValue(IWebElement row, string value, string camelCasePropertyName,
            bool isJsonObject = false)
        {
            var jsExecutor = (IJavaScriptExecutor)NSTestFramework.Browser.WebDriver;
            var editorCssSelector = $"input[name='{camelCasePropertyName}']";
            var gridElementSelector = $@"$($(""{editorCssSelector}"").parents('[kendo-grid]')[0])";

            row.FindElement(By.CssSelector(editorCssSelector));

            var changeInputEditValue = $@"$(""{editorCssSelector}"").val('{value}')";
            jsExecutor.ExecuteScript(changeInputEditValue);

            var triggerKendoEditDataChange = $@"
                var item = _.find({gridElementSelector}.data('kendoGrid').dataSource.data(), function(item){{
                    return item.uid == '{row.GetAttribute("data-uid")}';
                }}); 

                item.set('{camelCasePropertyName}', {(!isJsonObject ? "'" : string.Empty)}{value}{(!isJsonObject ? "'" : string.Empty)});
                item.trigger('change');";

            jsExecutor.ExecuteScript(triggerKendoEditDataChange);
        }

        public static void SetKendoGridInlineWithButtonsEditRowData(this IWebElement row, string property, string value)
        {
            AssertIsKendoGridRow(row);

            var camelCasePropertyName = $"{char.ToLower(property[0])}{property.Substring(1)}";

            var editControl = row.FindElement(By.CssSelector($"input[name='{camelCasePropertyName}']"));
            editControl.Clear();
            editControl.SendKeys(value);
        }

        public static string GetRowValue(this IWebElement row, string property)
        {
            AssertIsKendoGridRow(row);

            var camelCasePropertyName = $"{char.ToLower(property[0])}{property.Substring(1)}";

            var control = row.FindElement(By.CssSelector(
                $"[ng-bind-custom='dataItem.{camelCasePropertyName}'],[ng-bind='dataItem.{camelCasePropertyName}']"));
            return control.Text;
        }

        public static IWebElement GetUpdateButton(this IWebElement row)
        {
            AssertIsKendoGridRow(row);

            return row.FindElement(By.ClassName("k-grid-update"));
        }

        public static IWebElement GetEditButton(this IWebElement row)
        {
            AssertIsKendoGridRow(row);

            return row.FindElement(By.ClassName("k-grid-edit"));
        }

        private static void AssertIsKendoGrid(IWebElement control)
        {
            if (string.IsNullOrEmpty(control?.GetAttribute("kendo-grid")))
            {
                throw new InvalidOperationException("Element is not a kendo grid");
            }
        }

        private static void AssertIsKendoGridRow(IWebElement control)
        {
            var isKendoGridRow =
                NSTestFramework.Browser.WebDriver.ExecuteJavaScript<bool>(
                    $"return $(\"[data-uid='{control.GetAttribute("data-uid")}']\").parents('[kendo-grid]').length > 0");
            if (!isKendoGridRow)
                throw new InvalidOperationException("Element is not a kendo grid row");
        }

        private static ReadOnlyCollection<IWebElement> GetKendoGridRows(this IWebElement grid)
        {
            return grid.FindElements(By.CssSelector(".k-grid-content table tr"));
        }

        private static string GetPropertyName(IWebElement dataColumn)
        {
            var attributesToMatch = _dataColumnCssSelector.Split(',').Select(t => t.Replace("[", "").Replace("]", ""));
            return attributesToMatch
                .Select(attr => dataColumn.GetAttribute(attr)?.Replace("dataItem.", ""))
                .Select(prop => prop != null ? $"{prop.Capitalize()}" : string.Empty)
                .FirstOrDefault(prop => !string.IsNullOrEmpty(prop));
        }

        private static bool IsValidDataColumn(IWebElement dataColumn)
        {
            return dataColumn != null;
        }

        private static bool NotFound(IWebElement element)
        {
            return element == null;
        }

        private static TGridViewModel ParseRowData<TGridViewModel>(this IWebElement gridRow)
            where TGridViewModel : new()
        {
            var columns = GetRowCells(gridRow);
            var dataItem = new TGridViewModel();

            return ParseColumns(columns, dataItem);
        }

        private static TGridViewModel ParseRowData<TGridViewModel>(this IWebElement gridRow,
            List<KendoGridHeaderModel> headers)
            where TGridViewModel : class, new()
        {
            var columns = GetRowCells(gridRow);
            if (IsGroupingRow(gridRow))
                return null;

            var dataItem = new TGridViewModel();
            return ParseColumns(columns, dataItem, headers);
        }

        private static bool IsGroupingRow(IWebElement gridRow)
        {
            return gridRow.GetAttribute("class").Contains("k-grouping-row");
        }

        private static ReadOnlyCollection<IWebElement> GetRowCells(this IWebElement gridRow)
        {
            return gridRow.FindElements(By.CssSelector("td[role=\"gridcell\"]"));
        }

        private static TGridViewModel ParseColumns<TGridViewModel>(ReadOnlyCollection<IWebElement> columns,
            TGridViewModel dataItem)
        {
            foreach (var column in columns)
            {
                try
                {
                    var dataColumns = column.FindElements(By.CssSelector(_dataColumnCssSelector)).ToList();
                    dataItem = CreateRowModelBasedOnCssClasses(dataColumns, dataItem);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return dataItem;
        }

        private static TGridViewModel CreateRowModelBasedOnCssClasses<TGridViewModel>(IList<IWebElement> dataColumns,
            TGridViewModel dataItem)
        {
            foreach (var dataColumn in dataColumns)
            {
                if (!IsValidDataColumn(dataColumn))
                    continue;

                var propertyName = GetPropertyName(dataColumn);
                var value = dataColumn.Text;

                SetProperty(dataItem, propertyName, value);
            }

            return dataItem;
        }

        public static void SetProperty<T>(T obj, string propertyName, string value)
        {
            var type = typeof(T);

            if (!HasProperty(obj, propertyName)) return;

            if (DateTime.TryParseExact(value, "MM-dd-yyyy hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var date))
            {
                type.GetProperty(propertyName)?.SetValue(obj, date);
                return;
            }

            var propertyType = Nullable.GetUnderlyingType(type.GetProperty(propertyName).PropertyType) ??
                               type.GetProperty(propertyName).PropertyType;

            var safeValue = string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, propertyType);

            type.GetProperty(propertyName)?.SetValue(obj, safeValue);
        }

        private static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static int GetColumnIndexFromHeader(IWebElement column)
        {
            return int.Parse(column.GetAttribute("data-index"));
        }

        public static IWebElement GetColumnSpecifiedByIndex(this IWebElement grid, By gridSelector, int rowIndex, int columnIndex)
        {
            var rows = grid.GetAllRowsFromGrid(gridSelector);
            var cells = rows[rowIndex].FindElements(By.TagName("td"));
            return cells[columnIndex];
        }

        public static IWebElement GetLinkForSpecifiedColumnIndex(this IWebElement grid, By rowSelector, int rowIndex, int columnIndex)
        {
            var rows = grid.GetAllRowsFromGrid(rowSelector);
            var cells = rows[rowIndex].FindElements(By.TagName("td"));
            return cells[columnIndex].FindElement(By.TagName("a"));
        }

        public static ReadOnlyCollection<IWebElement> GetAllRowsFromGrid(this IWebElement grid, By gridSelector)
        {
            return grid.FindElements(gridSelector);
        }

        public static ReadOnlyCollection<IWebElement> GetAllRowsFromGridHelper(string gridClass)
        {
            return Browser.WebDriver.FindElement(By.ClassName(gridClass))
                .FindElements(By.CssSelector(".vgt-responsive>table>tbody>tr"));
        }

        public static IWebElement GetButtonWithSpecifiedLinkTextFromSpecifiedRow(this IWebElement grid, By gridSelector, string linkText,
            int rowIndex)
        {
            ReadOnlyCollection<IWebElement> rows = grid.GetAllRowsFromGrid(gridSelector);
            return rows[rowIndex].FindElement(By.LinkText(linkText));
        }

        public static int GetNumberOfItemsListedOnGridBottom(this IWebElement grid)
        {
            var numberOfItemsString = grid.FindElement(By.CssSelector("span.k-pager-info.k-label")).Text;
            if (numberOfItemsString.Contains("No items to display")) return 0;
            var numberOfItems = int.Parse(numberOfItemsString.Split(' ')[4]);
            return numberOfItems;
        }

        //Kendo Filtering

        public static void OpenFilteringDropdownForSpecifiedHeaderColumn(IWebElement columnDropdown)
        {
            columnDropdown.Click();
        }

        public static void SelectFilterOption()
        {
            var menu = Browser.WebDriver.FindElement(By.CssSelector("li.k-item.k-filter-item.k-state-default.k-last"));         
            menu.Focus();
            Thread.Sleep(1000);
            menu.Click();
        }

        public static void SendFilterCriteria(string criteria)
        {
            new WebDriverWait(Browser.WebDriver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(By.ClassName("k-textbox")));
            Browser.WebDriver.FindElement(By.ClassName("k-textbox")).Click();
            Browser.WebDriver.FindElement(By.ClassName("k-textbox")).SendKeys(criteria);
        }

        public static void SelectFilterButton()
        {
            var by = By.ClassName("k-primary");
            new WebDriverWait(Browser.WebDriver, TimeSpan.FromSeconds(30)).Until(ExpectedConditions.ElementToBeClickable(by));
            Thread.Sleep(1000);
            Browser.WebDriver.FindElement(by).Click();
            Thread.Sleep(1000);
        }

        public static void SelectDeleteButtonForSpecifiedFavoriteQuoteRow(IWebElement gridSelector, By rowSelector, int entryRowIndex)
        {
            gridSelector.GetButtonWithSpecifiedLinkTextFromSpecifiedRow(rowSelector, "DELETE", entryRowIndex).Click();
        }

        //Kendo Filtering

        public static int GetIndexForARowHavingASpecifiedColumnValue(IWebElement grid, By rowSelector, string columnName, int columnIndex)
        {
            var rows = grid.GetAllRowsFromGrid(rowSelector);
            for (var i = 1; i <= rows.Count; i++)
            {
                var cells = rows[i].FindElements(By.TagName("td"));
                if (cells[columnIndex].Text == columnName) return i;
            }
            return 0;
        }

        public static bool IsElementDisplayed(IWebElement element, By selector)
        {
            try
            {
                element.FindElement(selector);
                return element.Displayed;

            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static string SplitStringAfterASpecificCharacter(this string stringToBeSplit, char character)
        {
            return stringToBeSplit.Split(character)[1];
        }

        public static void ClearPreviousFilteringCriteria()
        {
            Browser.WebDriver.FindElement(By.CssSelector("li form div button:nth-child(2)")).Click();
        }

        public static void Focus(this IWebElement webElement)
        {
            var actions = new Actions(Browser.WebDriver);
            actions.MoveToElement(webElement).Build();
            actions.Perform();
        }

        public static void SwitchToLastOpenedTab(this IWebDriver driver)
        {
            var windowHandles = Browser.WebDriver.WindowHandles;
            var lastTabIndex = windowHandles.Count - 1;
            var lastTab = windowHandles[lastTabIndex];
            Browser.WebDriver.SwitchTo().Window(lastTab);
        }
    }
}
