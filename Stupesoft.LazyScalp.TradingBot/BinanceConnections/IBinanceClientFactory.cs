using Binance.Net.Interfaces.Clients;

namespace Stupesoft.LazyScalp.TradingBot.BinanceConnections;
public interface IBinanceClientFactory
{
    Task<IBinanceSocketClient> CreateSocketClientAsync();
    Task<IBinanceClient> CreateRestClientAsync();
}