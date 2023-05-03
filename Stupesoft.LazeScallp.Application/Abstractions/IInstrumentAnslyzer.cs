using Stupesoft.LazeScallp.Application.Models;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Abstractions;

public interface IInstrumentAnslyzer
{
    Task<List<InstrumentSignal>> AnalyzeAsync(FinInstrument instrument);
}
