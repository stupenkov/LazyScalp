using LiteDB.Async;
using Microsoft.Extensions.Configuration;
using Stupesoft.LazyScalp.Domain.Notifications;

namespace Stupesoft.LazyScalp.Infrastructure.Repositories;

public class NotificationRepository : INotificaitonRepository
{
    private readonly IConfiguration _configuration;

    public NotificationRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string ConnectionString => _configuration.GetConnectionString("App")!;

    public async Task AddAsync(Notification entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.EnsureIndexAsync(x => x.Ticker, true);
        await collection.UpsertAsync(entity);
    }

    public async Task AddOrUpdateAsync(Notification entity)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();

        await collection.EnsureIndexAsync(x => x.Ticker, true);
        if (entity.Id == 0)
            await collection.UpsertAsync(entity);
        else
            await collection.UpdateAsync(entity);
    }

    public async Task<Notification?> FindByNameAsync(string name)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.EnsureIndexAsync(x => x.Ticker);
        return await collection.FindOneAsync(x => x.Ticker == name);
    }

    public async Task UpdateAsync(Notification instrument)
    {
        using var db = new LiteDatabaseAsync(ConnectionString);
        var collection = db.GetCollection<Notification>();
        await collection.UpdateAsync(instrument);
    }
}

