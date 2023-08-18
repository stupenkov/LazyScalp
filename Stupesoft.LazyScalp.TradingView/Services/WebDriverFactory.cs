using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Stupesoft.LazyScalp.TradingView.Abstractions;

namespace Stupesoft.LazyScalp.TradingView.Services;

internal class WebDriverFactory : IWebDriverFactory, IDisposable
{
    private readonly IOptions<ScanerOptions> _scanerOptions;
    private HashSet<IWebDriver> _webDrivers = new();

    public WebDriverFactory(IOptions<ScanerOptions> scanerOptions)
    {
        _scanerOptions = scanerOptions;
    }

    public IWebDriver Create()
    {
        var options = new ChromeOptions();
        options.AddArgument("--lang=en-us");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--disable-web-security");
        options.AddArgument("--allow-running-insecure-content");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--force-color-profile=srgb");

        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
        if (_scanerOptions.Value.SkipAuth)
        {
            options.AddArgument(@$"--user-data-dir={_scanerOptions.Value.ChromeDataUserDir}");
            options.AddArgument(@$"--profile-directory={_scanerOptions.Value.ChromeProfile}");
        }
        else
        {
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("rofile.password_manager_enabled", false);
        }

        var webDriver = new ChromeDriver(options);
        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);
        webDriver.Manage().Window.Size = new(1980, 980);
        webDriver.Manage().Window.Position = new(0, 0);

        _webDrivers.Add(webDriver);
        return webDriver;
    }

    public void Destroy(IWebDriver webDriver)
    {
        _webDrivers.Remove(webDriver);
        webDriver.Dispose();
    }

    public void Dispose()
    {
        foreach (var webDriver in _webDrivers)
            webDriver?.Dispose();

        _webDrivers.Clear();
    }
}
