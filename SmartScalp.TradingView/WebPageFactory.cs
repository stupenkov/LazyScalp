using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace SmartScalp.TradingView;

public class WebPageFactory : IWebPageFactory
{
    private IWebDriver _webDriver;

    public WebPageFactory(IWebDriver webDriver)
    {
        _webDriver = webDriver;
    }

    public T Create<T>()
    {
        return PageFactory.InitElements<T>(_webDriver);
    }
}
