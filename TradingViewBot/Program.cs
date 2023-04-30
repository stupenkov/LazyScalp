using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using SmartScalp.TradingView;
using Telegram.Bot;
using TradingViewBot;

Console.WriteLine("Starting...");

// set options
using IHost host = Host.CreateDefaultBuilder(args).Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

var imagePreparationOptions = new ImagePreparationOptions();
config.GetSection("imagePreparation").Bind(imagePreparationOptions);

var indicatorOptions = new IndicatorOptions();
config.GetSection("indicator").Bind(indicatorOptions);

var tradingViewOptions = new TradingViewOptions();
config.GetSection("TradingView").Bind(tradingViewOptions);

var telegramBotOptions = new TelegramBotOptions();
config.GetSection("TelegramBot").Bind(telegramBotOptions);

TimeSpan repeateNotificationTime = TimeSpan.FromMinutes(60);

// create servicies
var dateTimeProvider = new DateTimeProvider();
var levelAnalyzer = new LevelAnalyzer(dateTimeProvider);
var imagePreparation = new ImagePreparation(imagePreparationOptions);
var telegramClient = new TelegramBotClient(telegramBotOptions.Token!);
var telegramm = new TelegramBot(telegramClient, telegramBotOptions.ChatId!);
var webDriverFactory = new WebDriverFactory();
IWebDriver webDriver = webDriverFactory.Create();
IWebPageFactory webPageFactory = new WebPageFactory(webDriver);
var screenshotAnalyzer = new ScreenshotAnalyzer(indicatorOptions);
var instrumentRepository = new MemoryInstrumentRepository();

// main
var chartPage = webPageFactory.Create<ChartPage>();
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
        await telegramm.SendAsync(new Telegram.TelegramMessage(preparatedImage, messageText));
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

        counter = counter >= numberOfInstrument ? 0 : counter + 1;
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

await host.RunAsync();