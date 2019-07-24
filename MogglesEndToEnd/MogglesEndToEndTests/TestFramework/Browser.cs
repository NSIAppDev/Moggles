using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MogglesEndToEndTests.TestFramework
{
    public static class Browser
    {
        private static IWebDriver _webDriver;

        public static void Goto(string url)
        {
            _webDriver.Navigate().GoToUrl(url);
        }

        public static ISearchContext Driver => _webDriver;

        public static IWebDriver WebDriver => _webDriver;

        public static void Close()
        {
            _webDriver.Quit();
        }

        public static void CreateNew(bool useHeadless = false)
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--ignore-certificate-errors");
            if (useHeadless)
            {
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("--headless");
            }

            _webDriver = new ChromeDriver(options);
            _webDriver.Manage().Window.Maximize();
        }
    }
}