using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.ScalpStations;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Servicies;
public class ScalpStationFilter : IInstrumentFilter
{
    private readonly IScalpStation _scalpStation;
    private readonly ITickerSymbolConverter _tickerSymbolConverter;
    private readonly IOptions<ScalpStationOptions> _options;
    private readonly ILogger<ScalpStationFilter> _logger;
    private List<SSInstruments> _instruments = new List<SSInstruments>();

    public ScalpStationFilter(
        IScalpStation scalpStation,
        ITickerSymbolConverter tickerSymbolConverter,
        IOptions<ScalpStationOptions> options,
        ILogger<ScalpStationFilter> logger)
    {
        _scalpStation = scalpStation;
        _tickerSymbolConverter = tickerSymbolConverter;
        _options = options;
        _logger = logger;
    }

    public Task<bool> FilterAsync(FinInstrument instrument)
    {
        bool result = _instruments
            .Select(x => _tickerSymbolConverter.ConvertToTradingViewFeatureSymbol(x.Symbol))
            .Select(x => x.ToUpper())
            .Any(x => x == instrument.Ticker.ToUpper());

        return Task.FromResult(result);
    }

    public Task UpdateAsync()
    {
        _instruments.Clear();
        foreach (var filter in _options.Value.Filters)
        {
            var httpRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForever(
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2),
                    onRetry: (ex, time) => _logger.LogError(ex, $"Retry {time}"));

            List<SSInstruments> instruments = new List<SSInstruments>();
            httpRetryPolicy.Execute(() =>
            {
                instruments = _scalpStation.GetInstrumentsAsync(filter.Top, filter.SortType, filter.Period).GetAwaiter().GetResult();
            });

            foreach (var instrumet in instruments)
            {
                if (!_instruments.Any(x => x.Symbol == instrumet.Symbol))
                {
                    _instruments.Add(instrumet);
                }
            }
        }

        return Task.CompletedTask;
    }
}
