using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.ScalpStations;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Servicies;
public class ScalpStationFilter : IInstrumentFilter
{
    private readonly IScalpStation _scalpStation;
    private readonly ITickerSymbolConverter _tickerSymbolConverter;
    private readonly IOptions<ScalpStationOptions> _options;
    private List<SSInstruments> _instruments = new List<SSInstruments>();

    public ScalpStationFilter(IScalpStation scalpStation, ITickerSymbolConverter tickerSymbolConverter, IOptions<ScalpStationOptions> options)
    {
        _scalpStation = scalpStation;
        _tickerSymbolConverter = tickerSymbolConverter;
        _options = options;
    }

    public Task<bool> FilterAsync(FinInstrument instrument)
    {
        bool result = _instruments
            .Select(x => _tickerSymbolConverter.ConvertToTradingViewFeatureSymbol(x.Symbol))
            .Select(x => x.ToUpper())
            .Any(x => x == instrument.Ticker.ToUpper());

        return Task.FromResult(result);
    }

    public async Task UpdateAsync()
    {
        _instruments.Clear();
        foreach (var filter in _options.Value.Filters)
        {
            var instruments = await _scalpStation.GetInstrumentsAsync(filter.Top, filter.SortType, filter.Period);
            foreach (var instrumet in instruments)
            {
                if (!_instruments.Any(x => x.Symbol == instrumet.Symbol))
                {
                    _instruments.Add(instrumet);
                }
            }
        }
    }
}
