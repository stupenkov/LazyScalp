using OpenQA.Selenium;

namespace SmartScalp.TradingView;

public interface IWebDriverFactory
{
    IWebDriver Create();
}
