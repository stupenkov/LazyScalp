using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.Domain.Instrument;
using Stupesoft.LazyScalp.Infrastructure;
using Stupesoft.LazyScalp.Infrastructure.Repositories;
using Stupesoft.LazyScalp.Infrastructure.Telegram;
using Stupesoft.LazyScalp.Infrastructure.TradingView;

Console.WriteLine("Starting...");

// create host
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => services
        .AddOptions()
        .Configure<ImagePreparationOptions>(hostContext.Configuration.GetSection(ImagePreparationOptions.ImagePreparation))
        .Configure<IndicatorOptions>(hostContext.Configuration.GetSection(IndicatorOptions.Indicator))
        .Configure<TelegramBotOptions>(hostContext.Configuration.GetSection(TelegramBotOptions.TelegramBot))
        .Configure<TradingViewOptions>(hostContext.Configuration.GetSection(TradingViewOptions.TradingView))
        .AddSingleton<IInstrumentRepository, MemoryInstrumentRepository>()
        .AddSingleton<IDateTimeProvider, DateTimeProvider>()
        .AddSingleton<IImagePreparation, ImagePreparation>()
        .AddSingleton<INotification, TelegramBot>()
        .AddSingleton<ILevelAnalyzer, LevelAnalyzer>()
        .AddSingleton<IScreenshotAnalyzer, ScreenshotAnalyzer>()
        .AddSingleton<ITradingView, ChartPage>()
        .AddSingleton<IWebDriverFactory, WebDriverFactory>()
        .AddSingleton<ITelegarmClientBotFactory, TelegramClientBotFactory>())
    .Build();

IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
TradingViewOptions tradingViewOptions = config.GetSection(TradingViewOptions.TradingView).Get<TradingViewOptions>()!;
TimeSpan repeateNotificationTime = TimeSpan.FromMinutes(60);

// get servicies
var dateTimeProvider = host.Services.GetRequiredService<IDateTimeProvider>();
var levelAnalyzer = host.Services.GetRequiredService<ILevelAnalyzer>();
var imagePreparation = host.Services.GetRequiredService<IImagePreparation>();
var telegramm = host.Services.GetService<INotification>();
var screenshotAnalyzer = host.Services.GetRequiredService<IScreenshotAnalyzer>();
var instrumentRepository = host.Services.GetRequiredService<IInstrumentRepository>();
var chartPage = host.Services.GetRequiredService<ITradingView>();

// main
await chartPage.LoginAsync(tradingViewOptions.Login!, tradingViewOptions.Password!);
await chartPage.SetChartTemplateAsync();


if (!await chartPage.IsOpenScreenerAsync())
{
    await chartPage.OpenScreenerAsync();
    await chartPage.RefreshPageAsync(); // fix съезжает панель скринира
}

await LoopAsync(Init, Loop);

async Task<int> Init()
{
    await RefreshPageAsync();
    await chartPage.UpdateScreenerDataAsync();
    int count = await chartPage.CountScreenerInstrumentsAsync();
    Console.WriteLine(count);
    return count;
}
async Task Loop(int counter)
{
    FinancialInstrument? financialInstrument = null;
    await chartPage.SelectInstrumentAsync(counter);

    await RepeatAsync(2, TimeSpan.FromSeconds(1), TimeSpan.Zero, async () =>
    {
        bool success = await RepeatAsync(3, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3), async () =>
        {
            financialInstrument = await chartPage.GetInstrumentAsync(counter);
            return await screenshotAnalyzer.IndicatorLoadedAsync(financialInstrument!.Screenshot);
        });

        if (success)
            return true;

        await RefreshPageAsync();
        return false;
    });

    if (financialInstrument is null)
        return;

    Console.WriteLine($"index: {counter}, name: {financialInstrument.Name}");

    byte[] preparatedImage = imagePreparation.Crop(financialInstrument.Screenshot);
    Signal signal = await screenshotAnalyzer.AnalyzeAsync(financialInstrument.Screenshot);
    Instrument instrument = await GetInstrumentOrCreate(financialInstrument);
    ResultOfLevel highResullt = levelAnalyzer.Analyze(instrument.HighLevel, instrument.HighDetectionTime, signal.HighLevel);
    ResultOfLevel lowResullt = levelAnalyzer.Analyze(instrument.LowLevel, instrument.LowDetectionTime, signal.LowLevel);

    bool isNotify = instrument.LastNotification.HasValue && instrument.LastNotification.Value + repeateNotificationTime < dateTimeProvider.GetCurrentTime();
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
        await telegramm.SendAsync(new NotificationMessage(preparatedImage, messageText));
        Console.WriteLine("Sent to telegram: " + messageText);
        instrument.LastNotification = dateTimeProvider.GetCurrentTime();
    }

    instrument.SetHighLevel(signal.HighLevel, dateTimeProvider.GetCurrentTime());
    instrument.SetLowLevel(signal.LowLevel, dateTimeProvider.GetCurrentTime());
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

async Task LoopAsync(Func<Task<int>> init, Func<int, Task> loop)
{
    int numberOfInstrument = 0;
    int counter = 0;
    while (true)
    {
        if (counter == 0)
        {
            Console.WriteLine("Loop initialization...");
            try
            {
                numberOfInstrument = await init.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loop initialization failed. \n{e}");
                Console.WriteLine($"Will try again, through 5 sec...");
                await Task.Delay(5000);
                continue;
            }
        }

        Console.WriteLine($"Loop invoke. Iterator index - {counter}...");
        try
        {
            await loop.Invoke(counter);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Loop invoke failed. /n{ex}");
            Console.WriteLine($"The iteration skipping.");
        }

        counter = counter >= numberOfInstrument - 1
            ? 0
            : counter + 1;
    }
}

async Task RefreshPageAsync()
{
    if (!await chartPage.IsOpenScreenerAsync())
    {
        await chartPage.OpenScreenerAsync();
    }

    await chartPage.RefreshPageAsync();
    await chartPage.InputTickerAsync("usdt.p");
}

async Task<Instrument> GetInstrumentOrCreate(FinancialInstrument financialInstrument)
{
    Instrument? instrument = await instrumentRepository.FindByNameAsync(financialInstrument.Name);

    if (instrument is null)
    {
        instrument = new Instrument(0, financialInstrument.Name);
        await instrumentRepository.AddAsync(instrument);
        Console.WriteLine($"Add instrument to repo");
    }

    return instrument;
}

