using Stupesoft.LazeScallp.Application.Models;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface ISender
{
    Task SendAsync(ICollection<InstrumentSignal> signals, FinInstrument instrument);
}
