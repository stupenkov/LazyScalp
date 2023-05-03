namespace Stupesoft.LazyScalp.Domain.Notifications;

public interface INotificationManager
{
    Notification Create(string ticker, DateTime? lastHigh = null, DateTime? lastLow = null);
}