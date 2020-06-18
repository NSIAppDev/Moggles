using NsTestFrameworkUI.Helpers;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;

namespace MogglesEndToEndTests.TestFramework
{
    public static class Utils
    {             
        public static bool IsLastUpdatedDateCorrectlyDisplayed(By cssSelector)
        {
            var dateAndTimeValue = Browser.WebDriver.FindElement(cssSelector).Text;
            var dateValue = dateAndTimeValue.Substring(dateAndTimeValue.IndexOf(":", StringComparison.Ordinal)+2);
            var formattedDateValue = DateTime.Parse(dateValue).Date;
            return formattedDateValue == DateTime.Now.Date;
        }
      
        public static ReadOnlyCollection<IWebElement> GetAllRowsFromGrid(this IWebElement grid, By rowSelector)
        {
            return grid.FindElements(By.CssSelector(".vgt-responsive> table > tbody"));
        }

        public static IWebElement GetColumnSpecifiedByIndex(this IWebElement grid, By rowSelector, int rowIndex, int columnIndex)
        {
            var rows = grid.GetAllRowsFromGrid(rowSelector);
            var cells = rows[rowIndex].FindElements(By.TagName("td"));
            return cells[columnIndex];
        }

        public static IWebElement GetHeaderSpecifiedByIndex(this IWebElement grid, int columnIndex)
        {
            var header = grid.FindElement(By.CssSelector(".vgt-responsive>table>thead> tr:nth-child(1)"));
            var cells = header.FindElements(By.TagName("th"));
            return cells[columnIndex];
        }
    }
}
