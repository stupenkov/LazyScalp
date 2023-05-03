namespace Stupesoft.LazyScalp.Domain.Notifications;

public interface INotificaitonRepository
{
    Task AddOrUpdateAsync(Notification entity);
    Task AddAsync(Notification entity);
    Task UpdateAsync(Notification entity);
    Task<Notification?> FindByNameAsync(string name);
}

