using Stupesoft.LazeScallp.Application.Abstractions;

namespace Stupesoft.LazyScalp.Infrastructure;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }
}