using Stupesoft.LazyScalp.Shared.Abstractions;

namespace Stupesoft.LazyScalp.Shared;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }
}