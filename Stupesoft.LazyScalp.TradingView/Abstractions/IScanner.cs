using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazyScalp.TradingView.Abstractions;

public interface IScanner
{
    bool IsPause { get; set; }

    event Action CompletedCycle;
    event Action<FinInstrumentTV> InstrumentReady;
    Task RunAsync(CancellationToken cancellationToken);
}
