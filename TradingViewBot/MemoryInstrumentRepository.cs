namespace TradingViewBot;

public class MemoryInstrumentRepository : IInstrumentRepository
{
    private readonly List<Instrument> _instruments = new();

    public Task AddAsync(Instrument instrument)
    {
        _instruments.Add(instrument);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        var entity = _instruments.Find(x => x.Id == id);
        if (entity != null)
        {
            _instruments.Remove(entity);
        }

        return Task.CompletedTask;
    }

    public Task<Instrument> GetAsync(int id)
    {
        return Task.FromResult(_instruments.Single(x => x.Id == id));
    }

    public Task<Instrument?> FindByNameAsync(string name)
    {
        return Task.FromResult(_instruments.SingleOrDefault(x => x.Name == name));
    }

    public Task UpdateAsync(Instrument instrument)
    {
        return Task.CompletedTask;
    }
}

