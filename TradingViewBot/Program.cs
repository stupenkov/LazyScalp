using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using SmartScalp.TradingView;
using Telegram.Bot;
using TradingViewBot;

Console.WriteLine("Starting...");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var imagePreparationOptions = new ImagePreparationOptions();
config.GetSection("imagePreparation").Bind(imagePreparationOptions);
var imagePreparation = new ImagePreparation(imagePreparationOptions);

var indicatorOptions = new IndicatorOptions();
config.GetSection("indicator").Bind(indicatorOptions);

var telegramClient = new TelegramBotClient("6182573890:AAE9eIjdIxc2jdPvxzv_MubCm4LdsxDu8Ew");
var telegramm = new TelegramBot(telegramClient, "@smartScalpX");

var webDriverFactory = new WebDriverFactory();
IWebDriver webDriver = webDriverFactory.Create();
IWebPageFactory webPageFactory = new WebPageFactory(webDriver);

var chartPage = webPageFactory.Create<ChartPage>();
await chartPage.LoginAsync("anton87.87@bk.ru", "9e67DFFDSd");
await chartPage.SetChartTemplateAsync();
if (await chartPage.IsOpenScreenerAsync())
    await chartPage.CloseScreenerAsync();

await chartPage.OpenScreenerAsync();
await chartPage.InputTicker("usdt.p");
await chartPage.UpdateScreenerDataAsync();
int count = await chartPage.CountScreenerInstrumentsAsync();
Console.WriteLine(count);
var screenshotAnalyzer = new ScreenshotAnalyzer(indicatorOptions);
var instrumentRepository = new MemoryInstrumentRepository();
var signalComparer = new SignalComparer();

for (int i = 0; i < count; i++)
{
    try
    {
        await chartPage.SelectInstrumentAsync(i);
    }
    catch (Exception e)
    {
        Console.WriteLine($"index: {i}, {e}");
        continue;
    }

    FinancialInstrument? financialInstrument = null;
    do
    {
        await Task.Delay(2000);
        try
        {
            financialInstrument = await chartPage.GetInstrumentAsync(i);
        }
        catch (Exception e)
        {
            Console.WriteLine($"index: {i}, {e}");
            break;
        }
    }
    while (!await screenshotAnalyzer.IndicatorLoadedAsync(financialInstrument!.Screenshot));

    if (financialInstrument is null)
        continue;

    var signal = await screenshotAnalyzer.AnalyzeAsync(financialInstrument.Screenshot);
    var instrument = await instrumentRepository.FindByNameAsync(financialInstrument.Name);
    var preparatedImage = imagePreparation.Crop(financialInstrument.Screenshot);
    if (instrument is null)
    {
        instrument = new Instrument
        {
            Id = 0,
            Name = financialInstrument.Name,
            HighLevel = signal.HighLevel,
            LowLevel = signal.LowLevel,
            LastUpdate = DateTime.UtcNow
        };

        await instrumentRepository.AddAsync(instrument);
        Console.WriteLine($"Add instrument to repo");

        if (await screenshotAnalyzer.HasSignalAsync(financialInstrument.Screenshot))
            await telegramm.SendAsync(new Telegram.TelegramMessage(preparatedImage, financialInstrument.Name));
    }

    SignalsType signals = signalComparer.Compare(new Signal(instrument.HighLevel, instrument.LowLevel), signal);
    if (signals.HasFlag(SignalsType.NewFormationLow | SignalsType.NewFormationHigh))
    {
        Console.WriteLine($"New formation");
        await telegramm.SendAsync(new Telegram.TelegramMessage(preparatedImage, $"{financialInstrument.Name}. Новая формация"));
        instrument.LowLevel = signal.LowLevel;
        instrument.HighLevel = signal.HighLevel;
        instrument.LastUpdate = DateTime.UtcNow;
    }
    else if (signals.HasFlag(SignalsType.LowComing | SignalsType.HighComing))
    {
        Console.WriteLine($"Coming to level");
        await telegramm.SendAsync(new Telegram.TelegramMessage(preparatedImage, $"{financialInstrument.Name}. Цена приближается к уровню"));
        instrument.LowLevel = signal.LowLevel;
        instrument.HighLevel = signal.HighLevel;
        instrument.LastUpdate = DateTime.UtcNow;
    }
    else if (signals.HasFlag(SignalsType.Repeat) && DateTime.UtcNow > instrument.LastUpdate + TimeSpan.FromMinutes(60))
    {
        Console.WriteLine($"Repeat");
        await telegramm.SendAsync(new Telegram.TelegramMessage(preparatedImage, $"{financialInstrument.Name}. Повтор"));
        instrument.LastUpdate = DateTime.UtcNow;
    }


    var result = await screenshotAnalyzer.HasSignalAsync(financialInstrument.Screenshot);
    //if (result)
    //    Console.Beep();
    Console.WriteLine($"index: {i}, name: {financialInstrument.Name}, has signal: {result}");

    if (i == count - 1)
    {
        i = -1;
        webDriver.Navigate().Refresh();
        try
        {
            webDriver.SwitchTo().Alert().Accept();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await Task.Delay(5000);
        await chartPage.UpdateScreenerDataAsync();
        await chartPage.InputTicker("usdt.p");
        count = await chartPage.CountScreenerInstrumentsAsync();
    }
}



