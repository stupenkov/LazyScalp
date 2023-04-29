namespace TradingViewBot;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }
}