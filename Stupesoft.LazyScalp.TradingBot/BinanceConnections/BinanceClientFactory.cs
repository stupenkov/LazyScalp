using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects;
using Microsoft.Extensions.Options;

namespace Stupesoft.LazyScalp.TradingBot.BinanceConnections;

public class BinanceClientFactory : IBinanceClientFactory
{
    private readonly IOptions<BinanceConnectionOptions> _binanceConnectionOptions;

    public BinanceClientFactory(IOptions<BinanceConnectionOptions> binanceConnectionOptions)
    {
        _binanceConnectionOptions = binanceConnectionOptions;
    }

    public Task<IBinanceSocketClient> CreateSocketClientAsync()
    {
        return Task.Factory.StartNew(() =>
        {
            return (IBinanceSocketClient)new BinanceSocketClient(new BinanceSocketClientOptions()
            {
                ApiCredentials = new BinanceApiCredentials(_binanceConnectionOptions.Value.Key!, _binanceConnectionOptions.Value.Secret!),
            });
        });
    }

    public Task<IBinanceClient> CreateRestClientAsync()
    {
        return Task.Factory.StartNew(() =>
        {
            return (IBinanceClient)new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new BinanceApiCredentials(_binanceConnectionOptions.Value.Key!, _binanceConnectionOptions.Value.Secret!),
            });
        });

    }
}
