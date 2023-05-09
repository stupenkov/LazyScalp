using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazyScalp.TradingBot;
using Stupesoft.LazyScalp.TradingBot.BinanceConnections;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => services
        .AddHostedService<MainHostedService>()
        .Configure<BinanceConnectionOptions>(hostContext.Configuration.GetSection(BinanceConnectionOptions.SectionName))
        .AddSingleton<IBinanceClientFactory, BinanceClientFactory>()
        .AddSingleton<BinanceClientProvider>()
        .AddSingleton<IBinanceClientProvider>(x => x.GetRequiredService<BinanceClientProvider>())
        .AddSingleton<IBinanceConnectionManager>(x => x.GetRequiredService<BinanceClientProvider>())
        .AddSingleton<ITrading, Trading>()
    )
    .Build();

await host.RunAsync();