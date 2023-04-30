namespace Stupesoft.LazeScallp.Application.Abstractions
{
    public interface INotification
    {
        Task SendAsync(NotificationMessage message);
    }
}
