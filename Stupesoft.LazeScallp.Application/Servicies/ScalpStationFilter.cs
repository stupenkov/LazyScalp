using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.ScalpStations;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using static Stupesoft.LazeScallp.Application.ScalpStations.ScalpStation;

namespace Stupesoft.LazeScallp.Application.Servicies;
public class ScalpStationFilter : IInstrumentFilter
{
    private readonly IScalpStation _scalpStation;
    private readonly ITickerSymbolConverter _tickerSymbolConverter;
    private List<SSInstruments> _instruments = new List<SSInstruments>();

    public ScalpStationFilter(IScalpStation scalpStation, ITickerSymbolConverter tickerSymbolConverter)
    {
        _scalpStation = scalpStation;
        _tickerSymbolConverter = tickerSymbolConverter;
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
        _instruments = await _scalpStation.GetInstrumentsAsync(16, SortType.Trades, "15m");
    }
}
