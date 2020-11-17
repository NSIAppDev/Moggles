using System;
using System.Collections.Generic;
using System.Linq;
using NsTestFrameworkUI.Helpers;
using OpenQA.Selenium;

namespace Moggles.EndToEndTests.TestFramework
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

        public static IWebElement GetHeaderSpecifiedByIndex(this IWebElement grid, int columnIndex)
        {
            var header = grid.FindElement(By.CssSelector(".vgt-responsive>table>thead> tr:nth-child(1)"));
            var cells = header.FindElements(By.TagName("th"));
            return cells[columnIndex];
        }

        public static void SelectOptionFromList(this By optionsList, string option)
        {
            IList<IWebElement> dropdownList = Browser.WebDriver.FindElements(optionsList);
            dropdownList.First(x => string.Equals(x.Text, option)).Click();
        }
    }
}
