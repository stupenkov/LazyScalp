using OpenQA.Selenium;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface IWebDriverFactory
{
    IWebDriver Create();
}
