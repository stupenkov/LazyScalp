using LiteDB.Async;
using Microsoft.Extensions.Configuration;
using Stupesoft.LazyScalp.Domain.FinInstruments;

namespace Stupesoft.LazyScalp.Infrastructure.Repositories;
public class FinInstrumentRepository : IFinInstrumentRepository
{
    private readonly IConfiguration _configuration;

    public FinInstrumentRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ConnectionString => _configuration.GetConnectionString("App")!;

    public async Task AddAsync(FinInstrument entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrument>();
        await collection.EnsureIndexAsync(x => x.Ticker, true);
        await collection.UpsertAsync(entity);
    }

    public async Task AddOrUpdateAsync(FinInstrument entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrument>();

        await collection.EnsureIndexAsync(x => x.Ticker, true);
        if (entity.Id == 0)
            await collection.UpsertAsync(entity);
        else
            await collection.UpdateAsync(entity);
    }

    public async Task<FinInstrument> FindByTickerAsync(string name)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrument>();
        await collection.EnsureIndexAsync(x => x.Ticker);
        return await collection.FindOneAsync(x => x.Ticker == name);
    }

    public async Task UpdateAsync(FinInstrument entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrument>();
        var result = await collection.UpdateAsync(entity);
    }
}
