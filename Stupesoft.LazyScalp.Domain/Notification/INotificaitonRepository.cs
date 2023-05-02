namespace Stupesoft.LazyScalp.Domain.Notification;

public interface INotificaitonRepository
{
    Task AddAsync(Notification instrument);
    Task UpdateAsync(Notification instrument);
    Task DeleteAsync(int id);
    Task<Notification> FindByNameAsync(string name);
}

