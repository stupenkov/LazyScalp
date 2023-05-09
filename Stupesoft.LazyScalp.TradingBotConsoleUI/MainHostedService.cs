using Binance.Net.Enums;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazyScalp.TradingBot;
using Stupesoft.LazyScalp.TradingBot.BinanceConnections;

internal class MainHostedService : BackgroundService
{
    private readonly ITrading _trading;
    private readonly IBinanceConnectionManager _binanceConnectionManager;
    private readonly IBinanceClientProvider _binanceClientProvider;

    public MainHostedService(ITrading trading, IBinanceConnectionManager binanceConnectionManager, IBinanceClientProvider binanceClientProvider)
    {
        _trading = trading;
        _binanceConnectionManager = binanceConnectionManager;
        _binanceClientProvider = binanceClientProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _binanceConnectionManager.Connect();

        Console.WriteLine("press key...");

        var xx = (await _binanceClientProvider.RestClient.UsdFuturesApi.Account.GetPositionInformationAsync()).Data.First(x => x.Symbol == "ARBUSDT");
        var xxx = (await _binanceClientProvider.RestClient.UsdFuturesApi.Trading.GetOpenOrdersAsync("ARBUSDT"));
        while (!stoppingToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.B)
            {
                var priceData = (await _binanceClientProvider.RestClient.UsdFuturesApi.ExchangeData.GetPriceAsync("ARBUSDT")).Data;
                var price = priceData.Price + priceData.Price * 0.0005m;
                await _trading.PlaceStopOrder(
                    "ARBUSDT",
                    OrderSide.Buy,
                    price,
                    4.6m,
                    0.4f,
                    0.4f);
            }

            await Task.Delay(100);
        }
    }
}