namespace Stupesoft.LazyScalp.TradingBot.BinanceConnections;

public interface IBinanceConnectionManager
{
    Task Connect();
    Task Disconnect();
    Task Reconnect();
}
