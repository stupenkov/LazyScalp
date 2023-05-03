using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface ITradingViewAPI
{
    public IScaner Scaner { get; }
    Task<List<FinInstrumentTV>> GetAllInstrumentsAsync();
}
