using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazeScallp.Application.Abstractions;
public interface IInstrumentFilter
{
    Task<bool> FilterAsync(FinInstrument instrument);
    Task UpdateAsync();
}
