namespace Stupesoft.LazyScalp.Domain.Notifications;

public class NotificationManager : INotificationManager
{
    public Notification Create(string ticker, DateTime? lastHigh = null, DateTime? lastLow = null)
    {
        if (string.IsNullOrEmpty(ticker))
            throw new ArgumentException($"'{nameof(ticker)}' cannot be null or empty.", nameof(ticker));

        return new Notification
        {
            Ticker = ticker,
            LastHigh = lastHigh,
            LastLow = lastLow
        };
    }
}

