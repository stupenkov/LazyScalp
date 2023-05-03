using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface IScaner
{
    event Action CompletedCycle;
    event Action<FinInstrumentTV> InstrumentReady;
    Task RunAsync(CancellationToken cancellationToken);
}
