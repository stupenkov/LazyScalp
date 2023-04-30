using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;

namespace SmartScalp.TradingView;

public class WebDriverFactory : IWebDriverFactory
{
    public IWebDriver Create()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;
        service.SuppressInitialDiagnosticInformation = true;

        var options = new ChromeOptions();
        options.AddArgument("--lang=en-us");
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--disable-web-security");
        options.AddArgument("--allow-running-insecure-content");
        //options.AddArgument("--start-maximized");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--force-color-profile=srgb");
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("rofile.password_manager_enabled", false);
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
        IWebDriver webDriver = new ChromeDriver(service, options);
        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        webDriver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30);
        webDriver.Manage().Window.Size = new(1980, 980);
        webDriver.Manage().Window.Position = new(0, 0);
        return webDriver;
    }
}
