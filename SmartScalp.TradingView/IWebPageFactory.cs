namespace SmartScalp.TradingView;

public interface IWebPageFactory
{
    T Create<T>();
}
