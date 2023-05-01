namespace Stupesoft.LazyScalp.Domain.Instrument;

public interface IInstrumentRepository
{
    Task AddAsync(Instrument instrument);
    Task UpdateAsync(Instrument instrument);
    Task DeleteAsync(int id);
    Task<Instrument> FindAsync(int id);
    Task<Instrument> FindByNameAsync(string name);
}

