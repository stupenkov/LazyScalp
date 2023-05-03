namespace Stupesoft.LazyScalp.Domain.FinInstruments;

public interface IFinInstrumentRepository
{
    Task AddAsync(FinInstrument entity);
    Task UpdateAsync(FinInstrument entity);
    Task AddOrUpdateAsync(FinInstrument entity);
    Task<FinInstrument> FindByTickerAsync(string name);
}
