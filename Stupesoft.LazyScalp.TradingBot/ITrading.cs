using Binance.Net.Enums;

namespace Stupesoft.LazyScalp.TradingBot;
public interface ITrading
{
    Task<bool> CancelOrderAsync(string symbolName, long orderId);
    Task GetMarketDataAsync();
    Task PlaceLimitOrder();
    Task<bool> PlaceStopOrder(string symbolName, OrderSide orderSide, decimal price, decimal quantity, float stopLossPercent, float takeProfitPercent);
}