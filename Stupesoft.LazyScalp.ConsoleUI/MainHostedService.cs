using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.Domain.Instrument;
using Stupesoft.LazyScalp.Domain.Notification;

namespace Stupesoft.LazyScalp.ConsoleUI;
public class MainHostedService : BackgroundService
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISignalAnalyser _signalAnalyser;
    private readonly IImagePreparation _imagePreparation;
    private readonly INotification _notification;
    private readonly IScreenshotAnalyzer _screenshotAnalyzer;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly INotificaitonRepository _notificaitonRepository;
    private readonly ITradingView _tradingView;
    private readonly IOptions<TradingViewOptions> _tradingVeiwOptions;
    private readonly ILogger<MainHostedService> _logger;

    public MainHostedService(
        IDateTimeProvider dateTimeProvider,
        ISignalAnalyser signalAnalyser,
        IImagePreparation imagePreparation,
        INotification notification,
        IScreenshotAnalyzer screenshotAnalyzer,
        IInstrumentRepository instrumentRepository,
        INotificaitonRepository notificaitonRepository,
        ITradingView tradingView,
        IOptions<TradingViewOptions> tradingVeiwOptions,
        ILogger<MainHostedService> logger)
    {
        _dateTimeProvider = dateTimeProvider;
        _signalAnalyser = signalAnalyser;
        _imagePreparation = imagePreparation;
        _notification = notification;
        _screenshotAnalyzer = screenshotAnalyzer;
        _instrumentRepository = instrumentRepository;
        _notificaitonRepository = notificaitonRepository;
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
            Notification notification = await GetNotificationOrCreate(financialInstrument);

            ResultOfSignals resultOfSignals = _signalAnalyser.Analyze(instrument, signal, notification);
            string messageText = $"{financialInstrument.Name}. ";

            DateTime currentTime = _dateTimeProvider.GetCurrentTime();
            bool needNotity = true;
            if (resultOfSignals.High.SignalType == SignalType.PriceReachedLevel || resultOfSignals.Low.SignalType == SignalType.PriceReachedLevel)
            {
                messageText += "Цена подошла близко к уровню.";
            }
            else if (resultOfSignals.High.SignalType == SignalType.PriceIsNearLevel || resultOfSignals.High.SignalType == SignalType.PriceIsNearLevel)
            {
                if (resultOfSignals.High.SignalType == SignalType.PriceIsNearLevel)
                {
                    messageText += $"Цена торгуется возле верхнего уровня {(int)resultOfSignals.High.NearLevelTime.TotalHours}ч.";
                    notification.SetLastTimeHigh(currentTime);
                }

                if (resultOfSignals.Low.SignalType == SignalType.PriceIsNearLevel)
                {
                    messageText += $"Цена торгуется возле нижнего уровня {(int)resultOfSignals.Low.NearLevelTime.TotalHours}ч.";
                    notification.SetLastTimeLow(currentTime);
                }
            }
            else
            {
                needNotity = false;
            }

            if (needNotity)
            {
                await _notification.SendAsync(new NotificationMessage(preparatedImage, messageText));
                _logger.LogInformation("Sent to telegram: " + messageText);

                if (resultOfSignals.High.SignalType == SignalType.PriceIsNearLevel)
                    notification.SetLastTimeHigh(currentTime);

                if (resultOfSignals.Low.SignalType == SignalType.PriceIsNearLevel)
                    notification.SetLastTimeLow(currentTime);
            }

            instrument.SetHighLevel(signal.HighLevel, currentTime);
            instrument.SetLowLevel(signal.LowLevel, currentTime);
            await _instrumentRepository.UpdateAsync(instrument);
            await _notificaitonRepository.UpdateAsync(notification);
        }
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

    private async Task RefreshPageAsync()
    {
        if (!await _tradingView.IsOpenScreenerAsync())
        {
            await _tradingView.OpenScreenerAsync();
        }

        await _tradingView.RefreshPageAsync();
        await _tradingView.InputTickerAsync("usdt.p");
    }

    private async Task<Instrument> GetInstrumentOrCreate(FinancialInstrument financialInstrument)
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

    private async Task<Notification> GetNotificationOrCreate(FinancialInstrument financialInstrument)
    {
        Notification? entity = await _notificaitonRepository.FindByNameAsync(financialInstrument.Name);

        if (entity is null)
        {
            entity = new Notification(0, financialInstrument.Name);
            await _notificaitonRepository.AddAsync(entity);
            _logger.LogInformation($"Add notification to repo");
        }

        return entity;
    }
}
