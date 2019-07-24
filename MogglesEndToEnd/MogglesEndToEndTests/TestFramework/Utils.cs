using OpenQA.Selenium;
using System;

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
    }
}
