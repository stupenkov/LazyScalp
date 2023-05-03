using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stupesoft.LazyScalp.Shared;
using Stupesoft.LazyScalp.Shared.Abstractions;
using Stupesoft.LazyScalp.Shared.Configurations;
using Stupesoft.LazyScalp.TradingView.Abstractions;
using Stupesoft.LazyScalp.TradingView.Domain;
using Stupesoft.LazyScalp.TradingView.Infrastracture;
using Stupesoft.LazyScalp.TradingView.Services;

namespace Stupesoft.LazyScalp.TradingView;

public static class TradingViewExtensions
{
    public static IHostBuilder AddTradingView(this IHostBuilder builder)
    {
        return builder.ConfigureServices((hostContext, services) => services
            .AddOptions()
            .Configure<IndicatorOptions>(hostContext.Configuration.GetSection(IndicatorOptions.SectionName).Bind)
            .Configure<TradingViewOptions>(hostContext.Configuration.GetSection(TradingViewOptions.TradingView).Bind)
            .AddSingleton<IWebDriverFactory, WebDriverFactory>()
            .AddSingleton<IFinInstrumentTVManager, FinInstrumentTVManager>()
            .AddSingleton<IPageChart, ChartPage>()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<ITradingViewAPI, TradingViewAPI>()
            .AddSingleton<IScanner, Scanner>()
            .AddSingleton<IFinInstrumentTVRepository, FinInstrumentTVRepository>());
    }
}
