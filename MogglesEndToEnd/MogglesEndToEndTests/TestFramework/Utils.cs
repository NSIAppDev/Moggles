using OpenQA.Selenium;
using System;
using System.Runtime.Remoting.Messaging;
using System.Windows.Media.Animation;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace MogglesEndToEndTests.TestFramework
{
    public class Utils
    {
        public static bool IsElementDisplayedOnScreen(By cssSelector)
        {
            try
            {
                Browser.WebDriver.FindElement(cssSelector);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool IsCheckboxChecked(By cssSelector)
        {
            return Browser.WebDriver.FindElement(cssSelector).Selected;
        }

        public static bool IsLastUpdatedDateCorrectlyDisplayed(By cssSelector)
        {
            var dateAndTimeValue = Browser.WebDriver.FindElement(cssSelector).Text;
            var dateValue = dateAndTimeValue.Substring(dateAndTimeValue.IndexOf(":", StringComparison.Ordinal)+2);
            var formattedDateValue = DateTime.Parse(dateValue).Date;
            return formattedDateValue == DateTime.Now.Date;
        }
    }
}
