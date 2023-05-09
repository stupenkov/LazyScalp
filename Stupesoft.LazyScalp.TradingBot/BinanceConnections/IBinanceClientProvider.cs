using Binance.Net.Interfaces.Clients;

namespace Stupesoft.LazyScalp.TradingBot.BinanceConnections;

public interface IBinanceClientProvider
{
    IBinanceClient RestClient { get; }
    IBinanceSocketClient SocketClient { get; }
    string ListenKey { get; }
}
