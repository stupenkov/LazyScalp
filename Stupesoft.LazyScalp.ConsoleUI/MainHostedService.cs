using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.Domain.Instrument;

namespace Stupesoft.LazyScalp.ConsoleUI;
public class MainHostedService : BackgroundService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILevelAnalyzer _levelAnalyzer;
    private readonly IImagePreparation _imagePreparation;
    private readonly INotification _notification;
    private readonly IScreenshotAnalyzer _screenshotAnalyzer;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly ITradingView _tradingView;
    private readonly IOptions<TradingViewOptions> _tradingVeiwOptions;
    private readonly ILogger<MainHostedService> _logger;

    public MainHostedService(
        IDateTimeProvider dateTimeProvider,
        ILevelAnalyzer levelAnalyzer,
        IImagePreparation imagePreparation,
        INotification notification,
        IScreenshotAnalyzer screenshotAnalyzer,
        IInstrumentRepository instrumentRepository,
        ITradingView tradingView,
        IOptions<TradingViewOptions> tradingVeiwOptions,
        ILogger<MainHostedService> logger)
    {
        _dateTimeProvider = dateTimeProvider;
        _levelAnalyzer = levelAnalyzer;
        _imagePreparation = imagePreparation;
        _notification = notification;
        _screenshotAnalyzer = screenshotAnalyzer;
        _instrumentRepository = instrumentRepository;
        _tradingView = tradingView;
        _tradingVeiwOptions = tradingVeiwOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TradingViewOptions tradingViewOptions = _tradingVeiwOptions.Value;
        TimeSpan repeateNotificationTime = TimeSpan.FromMinutes(60);

        await _tradingView.LoginAsync(tradingViewOptions.Login!, tradingViewOptions.Password!);
        await _tradingView.SetChartTemplateAsync();


        if (!await _tradingView.IsOpenScreenerAsync())
        {
            await _tradingView.OpenScreenerAsync();
            await _tradingView.RefreshPageAsync(); // fix съезжает панель скринира
        }

        await LoopAsync(Init, Loop, stoppingToken);

        async Task<int> Init()
        {
            await RefreshPageAsync();
            await _tradingView.UpdateScreenerDataAsync();
            int count = await _tradingView.CountScreenerInstrumentsAsync();
            _logger.LogInformation($"Total instruments: {count}");
            return count;
        }
        async Task Loop(int counter)
        {
            FinancialInstrument? financialInstrument = null;
            await _tradingView.SelectInstrumentAsync(counter);

            await RepeatAsync(2, TimeSpan.FromSeconds(1), TimeSpan.Zero, async () =>
            {
                bool success = await RepeatAsync(3, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), async () =>
                {
                    financialInstrument = await _tradingView.GetInstrumentAsync(counter);
                    return await _screenshotAnalyzer.IndicatorLoadedAsync(financialInstrument!.Screenshot);
                });

                if (success)
                    return true;

                await RefreshPageAsync();
                return false;
            });

            if (financialInstrument is null)
                return;

            _logger.LogInformation($"index: {counter}, name: {financialInstrument.Name}");

            byte[] preparatedImage = _imagePreparation.Crop(financialInstrument.Screenshot);
            Signal signal = await _screenshotAnalyzer.AnalyzeAsync(financialInstrument.Screenshot);
            Instrument instrument = await GetInstrumentOrCreate(financialInstrument);
            ResultOfLevel highResullt = _levelAnalyzer.Analyze(instrument.HighLevel, instrument.HighDetectionTime, signal.HighLevel);
            ResultOfLevel lowResullt = _levelAnalyzer.Analyze(instrument.LowLevel, instrument.LowDetectionTime, signal.LowLevel);

            bool isNotify = instrument.LastNotification.HasValue && instrument.LastNotification.Value + repeateNotificationTime < _dateTimeProvider.GetCurrentTime();
            string messageText = $"{financialInstrument.Name}. ";

            bool needSend = false;
            if (highResullt.SignalType == SignalType.PriceReachedLevel || lowResullt.SignalType == SignalType.PriceReachedLevel)
            {
                messageText += "Цена приближается к уровню.";
                needSend = true;
            }
            else if (highResullt.SignalType == SignalType.PriceApproachedLevel || lowResullt.SignalType == SignalType.PriceApproachedLevel)
            {
                messageText += "Цена находиться близко от уровня.";
                needSend = true;
            }
            else if (isNotify)
            {
                if (highResullt.SignalType == SignalType.PriceIsNearLevel)
                {
                    messageText += $"Цена находиться возле верхнего уровня {(int)highResullt.NearLevelTime.TotalHours}ч.";
                    needSend = true;
                }

                if (lowResullt.SignalType == SignalType.PriceIsNearLevel)
                {
                    messageText += $"Цена находиться возле нижнего уровня {(int)lowResullt.NearLevelTime.TotalHours}ч.";
                    needSend = true;
                }
            }

            if (needSend)
            {
                await _notification.SendAsync(new NotificationMessage(preparatedImage, messageText));
                _logger.LogInformation("Sent to telegram: " + messageText);

                instrument.LastNotification = _dateTimeProvider.GetCurrentTime();
            }

            instrument.SetHighLevel(signal.HighLevel, _dateTimeProvider.GetCurrentTime());
            instrument.SetLowLevel(signal.LowLevel, _dateTimeProvider.GetCurrentTime());
            await _instrumentRepository.UpdateAsync(instrument);
        }


    }

    async Task<bool> RepeatAsync(int count, TimeSpan timeout, TimeSpan startDelay, Func<Task<bool>> action)
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

    async Task LoopAsync(Func<Task<int>> init, Func<int, Task> loop, CancellationToken stoppingToken)
    {
        int numberOfInstrument = 0;
        int counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            if (counter == 0)
            {
                _logger.LogInformation("Loop initialization...");
                try
                {
                    numberOfInstrument = await init.Invoke();
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"Loop initialization failed. \n{e}");
                    _logger.LogInformation($"Will try again, through 5 sec...");
                    await Task.Delay(5000);
                    continue;
                }
            }

            _logger.LogInformation($"Loop invoke. Iterator index - {counter}...");
            try
            {
                await loop.Invoke(counter);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Loop invoke failed. /n{ex}");
                _logger.LogInformation($"The iteration skipping.");
            }

            counter = counter >= numberOfInstrument - 1
                ? 0
                : counter + 1;
        }
    }

    async Task RefreshPageAsync()
    {
        if (!await _tradingView.IsOpenScreenerAsync())
        {
            await _tradingView.OpenScreenerAsync();
        }

        await _tradingView.RefreshPageAsync();
        await _tradingView.InputTickerAsync("usdt.p");
    }

    async Task<Instrument> GetInstrumentOrCreate(FinancialInstrument financialInstrument)
    {
        Instrument? instrument = await _instrumentRepository.FindByNameAsync(financialInstrument.Name);

        if (instrument is null)
        {
            instrument = new Instrument(0, financialInstrument.Name);
            await _instrumentRepository.AddAsync(instrument);
            _logger.LogInformation($"Add instrument to repo");
        }

        return instrument;
    }
}
