using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Models;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Domain.Notifications;
using Stupesoft.LazyScalp.Shared.Abstractions;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class InstrumentAnalyzer : IInstrumentAnslyzer
{
    private readonly INotificaitonRepository _notificaitonRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly TimeSpan _lagNotificationTime;

    public InstrumentAnalyzer(
        INotificaitonRepository notificaitonRepository,
        IDateTimeProvider dateTimeProvider,
        IOptions<ApplicationOptions> appOptions)
    {
        _notificaitonRepository = notificaitonRepository;
        _dateTimeProvider = dateTimeProvider;
        _lagNotificationTime = TimeSpan.FromMinutes(appOptions.Value.NotificationDelayTimeMin);
    }

    public async Task<List<InstrumentSignal>> AnalyzeAsync(FinInstrument instrument)
    {
        Notification? notification = await _notificaitonRepository.FindByNameAsync(instrument.Ticker!);
        List<InstrumentSignal> signals = new();
        FinInstrument.Data currData = instrument.HistoryData[^1];

        if (currData.HighState == InstrumentState.ReachedLevel)
        {
            signals.Add(new(true, InstrumentState.ReachedLevel, "Цена достигла верхнего уровня."));
        }
        else if (currData.HighState == InstrumentState.TradingNearLevel)
        {
            if (_dateTimeProvider.GetCurrentTime() - notification!.LastHigh >= _lagNotificationTime)
            {
                TimeSpan? duration = _dateTimeProvider.GetCurrentTime() - instrument.HighDetectionLevelTime;
                var text = $"Инструмент тогруется возле верхнего уровня {TimeToStringFormat(duration)}.";
                signals.Add(new(true, InstrumentState.TradingNearLevel, text));
            }
        }

        if (currData.LowState == InstrumentState.ReachedLevel)
        {
            signals.Add(new(false, InstrumentState.ReachedLevel, "Цена достигла нижнего уровня."));
        }
        else if (currData.LowState == InstrumentState.TradingNearLevel)
        {
            if (_dateTimeProvider.GetCurrentTime() - notification!.LastLow >= _lagNotificationTime)
            {
                TimeSpan? duration = _dateTimeProvider.GetCurrentTime() - instrument.LowDetectionLevelTime;
                var text = $"Инструмент тогруется возле нижнего уровня {TimeToStringFormat(duration)}.";
                signals.Add(new(false, InstrumentState.TradingNearLevel, text));
            }
        }

        return signals;
    }

    private static string TimeToStringFormat(TimeSpan? time)
    {
        if (!time.HasValue)
            return string.Empty;

        string format = time.Value < TimeSpan.FromHours(1) ? @"mm\м" : @"h\ч\ mm\м";
        return time.Value.ToString(format);
    }
}