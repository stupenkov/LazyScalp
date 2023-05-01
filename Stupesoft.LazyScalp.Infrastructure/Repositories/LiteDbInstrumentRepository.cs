using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.Configuration;
using Stupesoft.LazyScalp.Domain.Instrument;

namespace Stupesoft.LazyScalp.Infrastructure.Repositories;

public class LiteDbInstrumentRepository : IInstrumentRepository
{
    private readonly IConfiguration _configuration;

    public LiteDbInstrumentRepository(IConfiguration configuration)
    {
        _configuration = configuration;

    }

    private string ConnectionString => _configuration.GetConnectionString("App")!;

    public async Task AddAsync(Instrument instrument)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Instrument>();
        await collection.EnsureIndexAsync(x => x.Name, true);
        await collection.UpsertAsync(instrument);
    }

    public async Task DeleteAsync(int id)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Instrument>();
        await collection.DeleteAsync(new BsonValue(id));
    }

    public async Task<Instrument> FindAsync(int id)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Instrument>();
        return await collection.FindByIdAsync(new BsonValue(id));
    }

    public async Task<Instrument> FindByNameAsync(string name)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Instrument>();
        await collection.EnsureIndexAsync(x => x.Name);
        return await collection.FindOneAsync(x => x.Name == name);
    }

    public async Task UpdateAsync(Instrument instrument)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Instrument>();
        await collection.UpdateAsync(instrument);
    }
}

