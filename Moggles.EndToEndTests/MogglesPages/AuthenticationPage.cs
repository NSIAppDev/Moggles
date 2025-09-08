using Moggles.EndToEndTests.TestFramework;
using NsTestFrameworkUI.Pages;
using OpenQA.Selenium;

namespace Moggles.EndToEndTests.MogglesPages
{
    public class AuthenticationPage
    {
        private readonly By _usernameInput = By.CssSelector("#i0116");
        private readonly By _nextButton = By.CssSelector("#idSIButton9");
        private readonly By _wnNumberInput = By.CssSelector("#username");
        private readonly By _passwordInput = By.CssSelector("#password");
        private readonly By _signOnButton = By.CssSelector("#signOnButton");
        private readonly By _optionNoButon = By.CssSelector("#idBtn_Back");


        public void Login()
        {
            _usernameInput.ActionSendKeys(Constants.MogglesUser);
            _nextButton.ActionClick();
            _wnNumberInput.ActionSendKeys(Constants.MogglesUser);
            _passwordInput.ActionSendKeys(Constants.MogglesPassword);
            _signOnButton.ActionClick();
            _optionNoButon.ActionClick();
        }
    }
}
