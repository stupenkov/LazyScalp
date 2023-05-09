using Binance.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;

namespace Stupesoft.LazyScalp.TradingBot.BinanceConnections;

public class BinanceClientProvider : IBinanceClientProvider, IBinanceConnectionManager
{
    private const string _connectionErrorMessage = "The connection was not established";
    private readonly IBinanceClientFactory _binanceConnectionFactory;
    private readonly ILogger<BinanceClientProvider> _logger;
    private IBinanceClient? _restClient;
    private IBinanceSocketClient? _socketClient;

    public BinanceClientProvider(IBinanceClientFactory binanceConnectionFactory, ILogger<BinanceClientProvider> logger)
    {
        _binanceConnectionFactory = binanceConnectionFactory;
        _logger = logger;
    }

    public IBinanceClient RestClient => _restClient ?? throw new Exception(_connectionErrorMessage);
    public IBinanceSocketClient SocketClient => _socketClient ?? throw new Exception(_connectionErrorMessage);
    public string ListenKey { get; private set; } = string.Empty;

    public async Task Connect()
    {
        _logger.LogInformation("Creating binance connection...");
        _restClient ??= await _binanceConnectionFactory.CreateRestClientAsync();
        _socketClient ??= await _binanceConnectionFactory.CreateSocketClientAsync();
        WebCallResult<string>? keyData;
        do
        {
            _logger.LogInformation("Starting user stream...");
            keyData = await _restClient.UsdFuturesApi.Account.StartUserStreamAsync();
            if (!keyData.Success)
            {
                _logger.LogWarning("Start user stream faild, try again...");
                await Task.Delay(5000);
            }
        }
        while (!keyData.Success);

        ListenKey = keyData.Data;
        _logger.LogInformation("Binance connections created");
    }

    public async Task Disconnect()
    {
        _logger.LogInformation("Disconnecting binance...");
        await Task.Factory.StartNew(() =>
        {
            _restClient?.Dispose();
            _socketClient?.Dispose();
        });
        _logger.LogInformation("Disconnect successful");
    }

    public async Task Reconnect()
    {
        _logger.LogInformation("Reconnecting to binance...");
        await Disconnect();
        _restClient = await _binanceConnectionFactory.CreateRestClientAsync();
        _socketClient = await _binanceConnectionFactory.CreateSocketClientAsync();
        _logger.LogInformation("Reconnection successful");
    }
}