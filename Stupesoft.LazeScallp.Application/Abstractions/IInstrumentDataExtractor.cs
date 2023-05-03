using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.TradingView.Domain;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface IInstrumentDataExtractor
{
    Task<FinInstrument.Data> ExtractAsync(FinInstrumentTV instrumentTradingView);
}
