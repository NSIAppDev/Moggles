using NSTestFrameworkDotNetCoreUI.Helpers;
using OpenQA.Selenium;
using System;

namespace MogglesEndToEndTests.TestFramework
{
    public class Utils
    {             
        public static bool IsLastUpdatedDateCorrectlyDisplayed(By cssSelector)
        {
            var dateAndTimeValue = Browser.WebDriver.FindElement(cssSelector).Text;
            var dateValue = dateAndTimeValue.Substring(dateAndTimeValue.IndexOf(":", StringComparison.Ordinal)+2);
            var formattedDateValue = DateTime.Parse(dateValue).Date;
            return formattedDateValue == DateTime.Now.Date;
        }
    }
}
