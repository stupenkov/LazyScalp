using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Models;

public record InstrumentSignal(bool IsHighLevel, InstrumentState State, string? Message);
