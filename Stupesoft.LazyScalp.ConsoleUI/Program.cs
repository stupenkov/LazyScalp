using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.ConsoleUI;
using Stupesoft.LazyScalp.Domain.Instrument;
using Stupesoft.LazyScalp.Infrastructure;
using Stupesoft.LazyScalp.Infrastructure.Repositories;
using Stupesoft.LazyScalp.Infrastructure.Telegram;
using Stupesoft.LazyScalp.Infrastructure.TradingView;

Console.WriteLine("Starting...");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => services
        .AddHostedService<MainHostedService>()
        .AddOptions()
        .Configure<ImagePreparationOptions>(hostContext.Configuration.GetSection(ImagePreparationOptions.ImagePreparation))
        .Configure<IndicatorOptions>(hostContext.Configuration.GetSection(IndicatorOptions.Indicator))
        .Configure<TelegramBotOptions>(hostContext.Configuration.GetSection(TelegramBotOptions.TelegramBot))
        .Configure<TradingViewOptions>(hostContext.Configuration.GetSection(TradingViewOptions.TradingView))
        .AddSingleton<IInstrumentRepository, LiteDbInstrumentRepository>()
        .AddSingleton<IDateTimeProvider, DateTimeProvider>()
        .AddSingleton<IImagePreparation, ImagePreparation>()
        .AddSingleton<INotification, TelegramBot>()
        .AddSingleton<ILevelAnalyzer, LevelAnalyzer>()
        .AddSingleton<IScreenshotAnalyzer, ScreenshotAnalyzer>()
        .AddSingleton<ITradingView, ChartPage>()
        .AddSingleton<IWebDriverFactory, WebDriverFactory>()
        .AddSingleton<ITelegarmClientBotFactory, TelegramClientBotFactory>())
    .Build();

await host.RunAsync();

