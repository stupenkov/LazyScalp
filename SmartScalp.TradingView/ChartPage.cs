using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics.CodeAnalysis;

namespace SmartScalp.TradingView;

public class ChartPage : ITradingView
{
    private const string PageUrl = "https://ru.tradingview.com/chart/";
    private readonly WebDriverWait _wait;
    private IWebDriver _webDriver;


    [FindsBy(How = How.XPath, Using = "/html/body/div[2]/div[4]/div/div/div/div")]
    [AllowNull]
    private IWebElement _humburgerButton;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"overlap-manager-root\"]/div/span/div[1]/div/div/div[8]")]
    [AllowNull]
    private IWebElement _enterItem;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"overlap-manager-root\"]/div/div[2]/div/div/div/div/div/div/div[1]/div[4]/div/span")]
    [AllowNull]
    private IWebElement _emailLoginButton;

    [FindsBy(How = How.XPath, Using = "//*[starts-with(@id, 'email-signin__user-name-input')]")]
    [AllowNull]
    private IWebElement _emailInput;

    [FindsBy(How = How.XPath, Using = "//*[starts-with(@id, 'email-signin__password-input')]")]
    [AllowNull]
    private IWebElement _passwordInput;

    [FindsBy(How = How.XPath, Using = "//*[starts-with(@id, 'email-signin__submit-button')]")]
    [AllowNull]
    private IWebElement _loginSubmitButton;

    [FindsBy(How = How.XPath, Using = "/html/body/div[2]/div[3]/div/div/div[3]/div[1]/div/div/div/div/div[14]/div/div/button[2]")]
    [AllowNull]
    private IWebElement _chartControlButton;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"overlap-manager-root\"]/div/span/div[1]/div/div/a")]
    [AllowNull]
    private IWebElement _firstChartTemplateItem;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"footer-chart-panel\"]/div[1]/div[1]/div[1]/button[1]")]
    [AllowNull]
    private IWebElement _openScreenerButton;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"footer-chart-panel\"]/div[2]/button[1]")]
    [AllowNull]
    private IWebElement _closeScreenerButton;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"bottom-area\"]/div[4]/div[4]/table/tbody")]
    [AllowNull]
    private IWebElement _screenerInstrumentsTable;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"bottom-area\"]/div[4]/div[2]/div[1]")]
    [AllowNull]
    private IWebElement _screenerUpdateDataButton;


    [FindsBy(How = How.XPath, Using = "/html/body/div[5]/div/div/div/article/button")]
    [AllowNull]
    private IWebElement _closeToastButton;

    [FindsBy(How = How.XPath, Using = "//*[@id=\"bottom-area\"]/div[4]/div[3]/table/thead/tr/th[1]/div/div/div[3]/input")]
    [AllowNull]
    private IWebElement _tickerInput;

    public ChartPage(IWebDriver webDriver)
    {
        _webDriver = webDriver;
        _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
    }

    public async Task LoginAsync(string login, string password)
    {
        _webDriver.Navigate().GoToUrl(PageUrl);
        _wait.Until(ExpectedConditions.ElementToBeClickable(_humburgerButton)).Click();
        _wait.Until(ExpectedConditions.ElementToBeClickable(_enterItem)).Click();
        _wait.Until(ExpectedConditions.ElementToBeClickable(_emailLoginButton)).Click();
        _emailInput.SendKeys(login);
        _passwordInput.SendKeys(password);
        _loginSubmitButton.Click();
        await Task.Delay(1000);
    }

    public Task SetChartTemplateAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_chartControlButton)).Click();
        _wait.Until(ExpectedConditions.ElementToBeClickable(_firstChartTemplateItem)).Click();
        return Task.CompletedTask;
    }

    public async Task OpenScreenerAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_openScreenerButton)).Click();
        await Task.Delay(1000);
    }

    public Task CloseScreenerAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_closeScreenerButton)).Click();
        return Task.CompletedTask;
    }

    public Task<int> CountScreenerInstrumentsAsync()
    {
        return Task.FromResult(_screenerInstrumentsTable.FindElements(By.TagName("tr")).Count);
    }

    public Task SelectInstrumentAsync(int index)
    {
        var tr = _screenerInstrumentsTable.FindElements(By.TagName("tr"))[index];
        _wait.Until(ExpectedConditions.ElementToBeClickable(tr)).Click();
        return Task.CompletedTask;
    }

    public Task UpdateScreenerDataAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_screenerUpdateDataButton)).Click();
        return Task.CompletedTask;
    }

    public Task<bool> IsOpenScreenerAsync()
    {
        return Task.FromResult(_screenerUpdateDataButton.Displayed);
    }

    public Task<FinancialInstrument> GetInstrumentAsync(int index)
    {
        var tr = _screenerInstrumentsTable.FindElements(By.TagName("tr"))[index];
        var title = tr.FindElement(By.CssSelector("div.tv-screener-table__symbol-container-description > div"));
        var image = TakeScreenshot();
        return Task.FromResult(new FinancialInstrument(title.Text, image));
    }

    public Task<bool> IsOpenAdsToastAsync()
    {
        return Task.FromResult(_webDriver.FindElement(By.XPath("/html/body/div[5]")).FindElements(By.TagName("div")).Count > 0);
    }

    public Task CloseAdsToastAsync()
    {
        _wait.Until(ExpectedConditions.ElementToBeClickable(_closeToastButton)).Click();
        return Task.CompletedTask;
    }

    public async Task InputTicker(string name)
    {
        _tickerInput.SendKeys(name);
        await Task.Delay(3000);
    }

    public async Task RefreshPage()
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

    private byte[] TakeScreenshot()
    {
        Screenshot ss = ((ITakesScreenshot)_webDriver).GetScreenshot();
        return ss.AsByteArray;
    }
}