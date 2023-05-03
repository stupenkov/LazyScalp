using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stupesoft.LazyScalp.TradingView.Abstractions;

namespace Stupesoft.LazyScalp.ConsoleUI;
internal class UIHostedService : BackgroundService
{
    private readonly ILogger<UIHostedService> _logger;
    private readonly IScanner _scaner;

    public UIHostedService(ILogger<UIHostedService> logger, IScanner scaner)
    {
        _logger = logger;
        _scaner = scaner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.P)
            {
                if (!_scaner.IsPause)
                {
                    _logger.LogInformation("PAUSE await...");
                    _scaner.IsPause = true;
                }
                else
                {
                    _logger.LogInformation("UNPAUSE");
                    _scaner.IsPause = false;
                }
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}
