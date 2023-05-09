using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Microsoft.Extensions.Logging;
using Stupesoft.LazyScalp.TradingBot.BinanceConnections;
using System.Text.Json;

namespace Stupesoft.LazyScalp.TradingBot;
public class Trading : ITrading
{
    private readonly IBinanceClientProvider _binanceClientProvider;
    private readonly ILogger<Trading> _logger;

    public Trading(IBinanceClientProvider binanceClientProvider, ILogger<Trading> logger)
    {
        _binanceClientProvider = binanceClientProvider;
        _logger = logger;
    }

    private decimal NormalizePrice(decimal price, decimal tickSize)
    {
        return (long)(price / tickSize) * tickSize;
    }

    public async Task<bool> PlaceStopOrder(
        string symbolName,
        OrderSide orderSide,
        decimal price,
        decimal quantity,
        float stopLossPercent,
        float takeProfitPercent)
    {
        var restClient = _binanceClientProvider.RestClient;
        var symbolData = (await restClient.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync()).Data.Symbols.Single(x => x.Name == symbolName);
        price = NormalizePrice(price, symbolData.PriceFilter!.TickSize);

        var order = await restClient.UsdFuturesApi.Trading.PlaceOrderAsync(
            symbolName,
            orderSide,
            FuturesOrderType.Stop,
            quantity,
            price: price,
            stopPrice: price);

        if (!order.Success)
        {
            _logger.LogWarning("Failed to place order {symbolName}. \n{error}", symbolName, order.Error?.Message);
            return false;
        }

        var sub = await _binanceClientProvider.SocketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(
          _binanceClientProvider.ListenKey,
          data =>
          {
              // Handle leverage update
          },
          data =>
          {
              // Handle margin update
          },
          data =>
          {
              // Handle account balance update, caused by trading
          },
          onOrderUpdate: async data =>
          {
              if (data.Data.UpdateData.OrderId != order.Data.Id || data.Data.UpdateData.ExecutionType != ExecutionType.New)
                  return;

              decimal price = data.Data.UpdateData.Price;
              decimal slPrice = Math.Round(price - (price * (decimal)stopLossPercent / 100), symbolData.PricePrecision);
              decimal tpPrice = Math.Round(price + (price * (decimal)takeProfitPercent / 100), symbolData.PricePrecision);
              var oppositeOrderSide = orderSide == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

              var slOrderTask = restClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                  symbolName,
                  oppositeOrderSide,
                  FuturesOrderType.StopMarket,
                  quantity: null,
                  closePosition: true,
                  stopPrice: slPrice);

              var tpOrderTask = restClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                  symbolName,
                  oppositeOrderSide,
                  FuturesOrderType.TakeProfitMarket,
                  quantity: null,
                  closePosition: true,
                  stopPrice: tpPrice);

              var results = await Task.WhenAll(slOrderTask, tpOrderTask);
              if (results.Any(x => !x.Success))
              {
                  foreach (var result in results)
                  {
                      if (!result.Success)
                      {
                          _logger.LogWarning("Failed to place order {symbolName}. \n{error}", symbolName, (results.First(x => !x.Success).Error?.Message));
                      }
                  }
              }
          },
          data =>
          {
              // Handle order update
          },
          data =>
          {
              // Handle order update
          },
          data =>
          {
              // Handle listen key expired
          });

        return true;
    }

    public Task<bool> CancelOrderAsync(string symbolName, long orderId)
    {
        throw new NotImplementedException();
    }

    public Task GetMarketDataAsync()
    {
        throw new NotImplementedException();
    }

    public Task PlaceLimitOrder()
    {
        throw new NotImplementedException();
    }
}
