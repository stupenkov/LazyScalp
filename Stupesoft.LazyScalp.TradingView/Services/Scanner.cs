﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.TradingView.Abstractions;
using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Services;

internal class Scanner : IScanner
{
    private readonly ILogger<Scanner> _logger;
    private readonly IOptions<TradingViewOptions> _tradingVeiwOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPageChart _tradingView;
    private readonly IOptions<ScanerOptions> _scanerOptions;
    private readonly IScreenshotAnalyzer _screenshotAnalyzer;
    private readonly IFinInstrumentTVRepository _finInstrumentTradingViewRepository;

    public bool IsPause { get; set; }

    public Scanner(
        ILogger<Scanner> logger,
        IOptions<TradingViewOptions> tradingVeiwOptions,
        IDateTimeProvider dateTimeProvider,
        IPageChart tradingView,
        IOptions<ScanerOptions> scanerOptions,
        IScreenshotAnalyzer screenshotAnalyzer,
        IFinInstrumentTVRepository finInstrumentTradingViewRepository)
    {
        _logger = logger;
        _tradingVeiwOptions = tradingVeiwOptions;
        _dateTimeProvider = dateTimeProvider;
        _tradingView = tradingView;
        _scanerOptions = scanerOptions;
        _screenshotAnalyzer = screenshotAnalyzer;
        _finInstrumentTradingViewRepository = finInstrumentTradingViewRepository;
    }

    public event Action? CompletedCycle;
    public event Action<FinInstrumentTV>? InstrumentReady;
    public event Action? BeforeRunCycle;

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        TradingViewOptions tradingViewOptions = _tradingVeiwOptions.Value;
        TimeSpan repeateNotificationTime = TimeSpan.FromMinutes(60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _tradingView.LoginAsync(tradingViewOptions.Login!, tradingViewOptions.Password!);
                await _tradingView.SetChartTemplateAsync();

                if (!await _tradingView.IsOpenScreenerAsync())
                {
                    await _tradingView.OpenScreenerAsync();
                }

                await LoopAsync(Init, Loop, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning("Trading view failed. Reload web driver through {time} sec...", _scanerOptions.Value.ReloadDelayTimeSecond * 1000);
                await Task.Delay(_scanerOptions.Value.ReloadDelayTimeSecond * 1000, stoppingToken);
                _tradingView.ReloadBrowser();
            }
        }
    }
    private async Task<int> Init()
    {
        BeforeRunCycle?.Invoke();
        await RefreshPageAsync();

        int count = await _tradingView.CountScreenerInstrumentsAsync();
        _logger.LogInformation("Total instruments: {count}", count);
        return count;
    }

    private async Task Loop(int counter)
    {
        FinInstrumentTV? finInstrument = null;
        await _tradingView.SelectInstrumentAsync(counter);

        await RepeatAsync(2, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), async () =>
        {
            bool success = await RepeatAsync(12, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), async () =>
            {
                finInstrument = await _tradingView.GetInstrumentAsync(counter);
                finInstrument.LastUpdate = _dateTimeProvider.GetCurrentTime();
                return await _screenshotAnalyzer.IndicatorLoadedAsync(finInstrument.ChartImage!);
            });

            if (success)
                return true;

            await RefreshPageAsync();
            return false;
        });

        if (finInstrument is null)
            return;

        _logger.LogInformation("Instrument has recivied: {Ticker}", finInstrument.Ticker);

        //await _finInstrumentTradingViewRepository.AddOrUpdateAsync(finInstrument);
        InstrumentReady?.Invoke(finInstrument);
    }

    private async Task<bool> RepeatAsync(int count, TimeSpan timeout, TimeSpan startDelay, Func<Task<bool>> action)
    {
        await Task.Delay(startDelay);
        for (int i = 0; i < count; i++)
        {
            bool result = await action.Invoke();
            if (result)
                return true;

            await Task.Delay(timeout);
        }

        return false;
    }

    private async Task LoopAsync(Func<Task<int>> init, Func<int, Task> loop, CancellationToken stoppingToken)
    {
        int numberOfInstrument = 0;
        int counter = 0;
        bool showedMessage = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsPause)
            {
                if (!showedMessage)
                {
                    _logger.LogInformation("Scanner paused.");
                    showedMessage = true;
                }

                await Task.Delay(100, stoppingToken);
                continue;
            }

            showedMessage = false;
            if (counter == 0)
            {
                _logger.LogInformation("Loop initialization...");
                try
                {
                    numberOfInstrument = await init.Invoke();
                }
                catch (Exception e)
                {
                    _logger.LogError("Loop initialization failed. \n{e}", e);
                    throw;
                }
            }

            _logger.LogInformation("Loop invoke. Iterator index {counter} of {numberOfInstrument}...", counter + 1, numberOfInstrument);
            try
            {
                await loop.Invoke(counter);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Loop invoke failed. \n{ex}", ex);
                throw;
            }

            if (counter >= numberOfInstrument - 1)
            {
                counter = 0;
                CompletedCycle?.Invoke();
            }
            else
            {
                counter++;
            }
        }
    }

    private async Task RefreshPageAsync()
    {
        if (!await _tradingView.IsOpenScreenerAsync())
        {
            await _tradingView.OpenScreenerAsync();
        }

        await _tradingView.RefreshPageAsync();
        await _tradingView.InputTickerAsync("usdt.p");
    }
}
