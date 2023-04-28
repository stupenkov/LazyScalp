namespace TradingViewBot;

public interface IInstrumentRepository
{
    Task AddAsync(Instrument instrument);
    Task UpdateAsync(Instrument instrument);
    Task DeleteAsync(int id);
    Task<Instrument> GetAsync(int id);
    Task<Instrument?> FindByNameAsync(string name);
}

