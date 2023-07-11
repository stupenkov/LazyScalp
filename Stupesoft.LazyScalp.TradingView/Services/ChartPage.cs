using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Stupesoft.LazyScalp.TradingView.Abstractions;
using Stupesoft.LazyScalp.TradingView.Domain;
using Microsoft.Extensions.Options;

namespace Stupesoft.LazyScalp.TradingView.Services;

internal class ChartPage : IPageChart
{
    private const string _pageUrl = "https://ru.tradingview.com/chart/";
    private readonly WebDriverWait _wait;
    private readonly IWebDriverFactory _webDriverFactory;
    private readonly IFinInstrumentTVManager _finInstrumentTradingViewManager;
    private readonly IOptions<ScanerOptions> _scanerOptions;
    private readonly Func<IWebElement> _humburgerButton;
    private readonly Func<IWebElement> _enterItem;
    private readonly Func<IWebElement> _emailLoginButton;
    private readonly Func<IWebElement> _emailInput;
    private readonly Func<IWebElement> _passwordInput;
    private readonly Func<IWebElement> _loginSubmitButton;
    private readonly Func<IWebElement> _chartControlButton;
    private readonly Func<IWebElement> _firstChartTemplateItem;
    private readonly Func<IWebElement> _openScreenerButton;
    private readonly Func<IWebElement> _closeScreenerButton;
    private readonly Func<IWebElement> _screenerInstrumentsTable;
    private readonly Func<IWebElement> _screenerUpdateDataButton;
    private readonly Func<IWebElement> _togglePanelButton;
    private readonly Func<IWebElement> _closeToastButton;
    private readonly Func<IWebElement> _tickerInput;
    private IWebDriver _webDriver;

    public ChartPage(IWebDriverFactory webDriverFactory, IFinInstrumentTVManager finInstrumentTradingViewManager, IOptions<ScanerOptions> scanerOptions)
    {
        _webDriverFactory = webDriverFactory;
        _finInstrumentTradingViewManager = finInstrumentTradingViewManager;
        _scanerOptions = scanerOptions;

        _webDriver = webDriverFactory.Create();
        _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
        _humburgerButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.HumburgerButton));
        _enterItem = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.EnterItem));
        _emailLoginButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.EmailLoginButton));
        _emailInput = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.EmailInput));
        _passwordInput = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.PasswordInput));
        _loginSubmitButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.LoginSubmitButton));
        _chartControlButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.ChartControlButton));
        _firstChartTemplateItem = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.FirstChartTemplateItem));
        _openScreenerButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.OpenScreenerButton));
        _closeScreenerButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.CloseScreenerButton));
        _screenerInstrumentsTable = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.ScreenerInstrumentsTable));
        _screenerUpdateDataButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.ScreenerUpdateDataButton));
        _togglePanelButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.TogglePanelButton));
        _closeToastButton = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.CloseToastButton));
        _tickerInput = () => _webDriver.FindElement(By.XPath(_scanerOptions.Value.Selectors!.TickerInput));
    }

    public async Task LoginAsync(string login, string password)
    {
        _webDriver.Navigate().GoToUrl(_pageUrl);
        if (!_scanerOptions.Value.SkipAuth)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(_humburgerButton())).Click();
            _wait.Until(ExpectedConditions.ElementToBeClickable(_enterItem())).Click();
            _wait.Until(ExpectedConditions.ElementToBeClickable(_emailLoginButton())).Click();
            _emailInput().SendKeys(login);
            _passwordInput().SendKeys(password);
            _loginSubmitButton().Click();
        }

        await Task.Delay(1000);
    }

    public async Task SetChartTemplateAsync()
    {
        await WaitAndRefreshAsync(() =>
        {
            if (!_scanerOptions.Value.SkipAuth)
            {
                _wait.Until(ExpectedConditions.ElementToBeClickable(_chartControlButton())).Click();
                _wait.Until(ExpectedConditions.ElementToBeClickable(_firstChartTemplateItem())).Click();
            }
        });
    }

    public async Task OpenScreenerAsync()
    {
        await WaitAndRefreshAsync(() =>
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(_openScreenerButton())).Click();
        });

        await Task.Delay(1000);
    }

    public async Task CloseScreenerAsync()
    {
        await WaitAndRefreshAsync(_wait.Until(ExpectedConditions.ElementToBeClickable(_closeScreenerButton())).Click);
    }

    public Task<int> CountScreenerInstrumentsAsync()
    {
        return Task.FromResult(_screenerInstrumentsTable().FindElements(By.TagName("tr")).Count);
    }

    public async Task SelectInstrumentAsync(int index)
    {
        var tr = _screenerInstrumentsTable().FindElements(By.TagName("tr"))[index];
        _wait.Until(ExpectedConditions.ElementToBeClickable(tr)).Click();
        await Task.Delay(1000);
    }

    public async Task UpdateScreenerDataAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_screenerUpdateDataButton())).Click();
        await Task.Delay(3000);
    }

    public async Task<bool> IsOpenScreenerAsync()
    {
        await Task.Delay(1);
        return _wait.Until(ExpectedConditions.ElementToBeClickable(_togglePanelButton())).GetAttribute("data-active") == "false";
    }

    public async Task<FinInstrumentTV> GetInstrumentAsync(int index)
    {
        var tr = _screenerInstrumentsTable().FindElements(By.TagName("tr"))[index];
        string title = tr.FindElement(By.CssSelector("div.tv-screener-table__symbol-container-description > div")).Text;
        byte[] image = TakeScreenshot();
        await Task.Delay(1);

        return _finInstrumentTradingViewManager.Create(image, title);
    }

    public async Task<bool> IsOpenAdsToastAsync()
    {
        bool? result = null;
        await WaitAndRefreshAsync(() =>
        {
            result = _webDriver.FindElement(By.XPath("/html/body/div[5]")).FindElements(By.TagName("div")).Count > 0;
        });

        return result!.Value;
    }

    public Task CloseAdsToastAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_closeToastButton())).Click();
        return Task.CompletedTask;
    }

    public async Task InputTickerAsync(string name)
    {
        await WaitAndRefreshAsync(() =>
        {
            _tickerInput().Clear();
            _tickerInput().SendKeys(name);
        });

        await Task.Delay(3000);
    }

    public async Task RefreshPageAsync()
    {
        _webDriver.Navigate().Refresh();
        try
        {
            _webDriver.SwitchTo().Alert().Accept();
        }
        catch (Exception)
        {
        }

        await Task.Delay(0);
    }

    public void ReloadBrowser()
    {
        _webDriverFactory.Destroy(_webDriver);
        _webDriver = _webDriverFactory.Create();
    }

    private byte[] TakeScreenshot()
    {
        Screenshot ss = ((ITakesScreenshot)_webDriver).GetScreenshot();
        return ss.AsByteArray;
    }

    private async Task WaitAndRefreshAsync(Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (WebDriverTimeoutException)
        {
            await RefreshPageAsync();
            action?.Invoke();
        }
    }
}
