using LiteDB.Async;
using Microsoft.Extensions.Configuration;
using Stupesoft.LazyScalp.TradingView.Domain;
using System.IO;

namespace Stupesoft.LazyScalp.TradingView.Infrastracture;

internal class FinInstrumentTVRepository : IFinInstrumentTVRepository
{
    private readonly IConfiguration _configuration;

    public FinInstrumentTVRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ConnectionString => _configuration.GetConnectionString("TradingViewDB")!;

    public async Task AddOrUpdateAsync(FinInstrumentTV entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrumentTV>();
        await collection.EnsureIndexAsync(x => x.Ticker, true);
        if (entity.Id == 0)
            await collection.UpsertAsync(entity);
        else
            await collection.UpdateAsync(entity);
    }

    public async Task<FinInstrumentTV> FindByTickerAsync(string name)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrumentTV>();
        await collection.EnsureIndexAsync(x => x.Ticker);
        return await collection.FindOneAsync(x => x.Ticker == name);
    }

    public Task<IEnumerable<FinInstrumentTV>> GetAllAsync()
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<FinInstrumentTV>();
        return collection.FindAllAsync();
    }
}
