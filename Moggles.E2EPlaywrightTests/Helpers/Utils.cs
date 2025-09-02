using Microsoft.Playwright;

namespace Moggles.E2EPlaywrightTests.Helpers
{
    public class Utils(IPage page)
    {
        public async Task<bool> IsLastUpdatedDateCorrectlyDisplayedAsync(ILocator selector)
        {
            var dateAndTimeValue = await selector.InnerTextAsync();
            var dateValue = dateAndTimeValue[(dateAndTimeValue.IndexOf(":", StringComparison.Ordinal) + 2)..];
            var formattedDateValue = DateTime.Parse(dateValue).Date;
            return formattedDateValue == DateTime.Now.Date;
        }

        public async Task<ILocator> GetHeaderSpecifiedByIndexAsync(string gridSelector, int columnIndex)
        {
            var header = page.Locator($"{gridSelector} .vgt-responsive>table>thead> tr:nth-child(1)");
            var cell = header.Locator("th").Nth(columnIndex);
            return cell;
        }

        public async Task SelectOptionFromListAsync(ILocator optionsListSelector, string option)
        {
            var optionElement = optionsListSelector.Filter(new() { HasText = option }).First;
            await optionElement.ClickAsync();
        }

        public async Task SelectFromDropdownAsync(ILocator dropdownSelector, ILocator optionsListSelector, string option)
        {
            await dropdownSelector.ClickAsync();
            var optionElement = optionsListSelector.Filter(new() { HasText = option }).First;
            await optionElement.ClickAsync();
        }
    }

}
