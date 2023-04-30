namespace Stupesoft.LazyScalp.Infrastructure.TradingView;

public interface IWebPageFactory
{
    T Create<T>();
}
