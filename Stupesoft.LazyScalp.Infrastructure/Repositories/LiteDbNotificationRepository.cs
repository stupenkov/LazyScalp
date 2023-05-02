using LiteDB;
using LiteDB.Async;
using Microsoft.Extensions.Configuration;
using Stupesoft.LazyScalp.Domain.Notification;

namespace Stupesoft.LazyScalp.Infrastructure.Repositories;

public class LiteDbNotificationRepository : INotificaitonRepository
{
    private readonly IConfiguration _configuration;

    public LiteDbNotificationRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ConnectionString => _configuration.GetConnectionString("App")!;

    public async Task AddAsync(Notification entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.EnsureIndexAsync(x => x.InstrumentName, true);
        await collection.UpsertAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.DeleteAsync(new BsonValue(id));
    }

    public async Task<Notification> FindByNameAsync(string name)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.EnsureIndexAsync(x => x.InstrumentName);
        return await collection.FindOneAsync(x => x.InstrumentName == name);
    }

    public async Task UpdateAsync(Notification instrument)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.UpdateAsync(instrument);
    }
}

