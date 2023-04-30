using OpenQA.Selenium;

namespace Stupesoft.LazyScalp.Infrastructure.TradingView;

public interface IWebDriverFactory
{
    IWebDriver Create();
}
