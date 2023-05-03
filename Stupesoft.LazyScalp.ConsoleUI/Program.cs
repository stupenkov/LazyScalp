using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Servicies;
using Stupesoft.LazyScalp.ConsoleUI;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Domain.Notifications;
using Stupesoft.LazyScalp.Infrastructure.Repositories;
using Stupesoft.LazyScalp.Infrastructure.Telegram;
using Stupesoft.LazyScalp.Shared;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.Shared.Configurations;
using Stupesoft.LazyScalp.Shared.Services;
using Stupesoft.LazyScalp.TradingView;

Console.WriteLine("Starting...");

using IHost host = Host.CreateDefaultBuilder(args)
    .AddTradingView()
    .ConfigureServices((hostContext, services) => services
        .AddHostedService<MainHostedService>()
        .AddHostedService<UIHostedService>()
        .Configure<ImagePreparationOptions>(hostContext.Configuration.GetSection(ImagePreparationOptions.SectionName))
        .Configure<TelegramBotOptions>(hostContext.Configuration.GetSection(TelegramBotOptions.SectionName))
        .Configure<IndicatorOptions>(hostContext.Configuration.GetSection(IndicatorOptions.SectionName))
        .Configure<ApplicationOptions>(hostContext.Configuration.GetSection(ApplicationOptions.SectionName))
        .AddSingleton<INotificaitonRepository, NotificationRepository>()
        .AddSingleton<INotificationManager, NotificationManager>()
        .AddSingleton<IInstrumentAnslyzer, InstrumentAnalyzer>()
        .AddSingleton<ISender, Sender>()
        .AddSingleton<IFinInstrumentRepository, FinInstrumentRepository>()
        .AddSingleton<IDateTimeProvider, DateTimeProvider>()
        .AddSingleton<IFinInstrumentManager, FinInstrumentManager>()
        .AddSingleton<IImagePreparation, ImagePreparation>()
        .AddSingleton<INotification, TelegramBot>()
        .AddSingleton<IInstrumentDataExtractor, InstrumentDataExtractor>()
        .AddSingleton<IScreenshotAnalyzer, ScreenshotAnalyzer>()
        .AddSingleton<ITelegarmClientBotFactory, TelegramClientBotFactory>())
    .Build();

await host.RunAsync();

