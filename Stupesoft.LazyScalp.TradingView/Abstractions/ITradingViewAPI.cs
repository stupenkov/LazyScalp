using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface ITradingViewAPI
{
    public IScanner Scaner { get; }
    Task<List<FinInstrumentTV>> GetAllInstrumentsAsync();
}
