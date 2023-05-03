namespace Stupesoft.LazyScalp.Domain.Notifications;

public class Notification
{
    public int Id { get; set; }
    public string? Ticker { get; set; }
    public DateTime? LastHigh { get; set; }
    public DateTime? LastLow { get; set; }
}

