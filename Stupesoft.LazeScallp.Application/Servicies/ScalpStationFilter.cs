using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.ScalpStations;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using static Stupesoft.LazeScallp.Application.ScalpStations.ScalpStation;

namespace Stupesoft.LazeScallp.Application.Servicies;
public class ScalpStationFilter : IInstrumentFilter
{
    private readonly IScalpStation _scalpStation;
    private readonly ITickerSymbolConverter _tickerSymbolConverter;

    public ScalpStationFilter(IScalpStation scalpStation, ITickerSymbolConverter tickerSymbolConverter)
    {
        _scalpStation = scalpStation;
        _tickerSymbolConverter = tickerSymbolConverter;
    }

    public async Task<bool> FilterAsync(FinInstrument instrument)
    {
        var scalpStationList = await _scalpStation.GetInstrumentsAsync(16, SortType.Trades, "15m");

        return scalpStationList
            .Select(x => _tickerSymbolConverter.ConvertToTradingViewFeatureSymbol(x.Symbol))
            .Select(x => x.ToUpper())
            .Any(x => x == instrument.Ticker.ToUpper());
    }
}
