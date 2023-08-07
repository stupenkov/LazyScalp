using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Models;
using Stupesoft.LazyScalp.Domain.FinInstruments;
using Stupesoft.LazyScalp.Domain.Notifications;
using Stupesoft.LazyScalp.Shared.Abstractions;

namespace Stupesoft.LazeScallp.Application.Servicies;

public class Sender : ISender
{
    private readonly INotificaitonRepository _notificaitonRepository;
    private readonly INotificationManager _notificationManager;
    private readonly IImagePreparation _imagePreparation;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly INotification _notification;

    public Sender(
        INotificaitonRepository notificaitonRepository,
        INotificationManager notificationManager,
        IImagePreparation imagePreparation,
        IDateTimeProvider dateTimeProvider,
        INotification notification)
    {
        _notificaitonRepository = notificaitonRepository;
        _notificationManager = notificationManager;
        _imagePreparation = imagePreparation;
        _dateTimeProvider = dateTimeProvider;
        _notification = notification;
    }

    public async Task SendAsync(ICollection<InstrumentSignal> signals, FinInstrument instrument)
    {
        Notification? notification = await _notificaitonRepository.FindByNameAsync(instrument.Ticker!);
        notification ??= _notificationManager.Create(instrument.Ticker!);
        DateTime currDate = _dateTimeProvider.GetCurrentTime();

        if (signals.Any(x => x.State == InstrumentState.ReachedLevel || x.State == InstrumentState.TradingNearLevel))
        {
            var image = _imagePreparation.Crop(instrument.HistoryData[^1].ChartImage!);
            var text = $"`{instrument.Ticker}` " + string.Join(' ', signals.Select(x => x.Message));

            await _notification.SendAsync(new NotificationMessage(image, text));

            if (signals.Any(x => x.IsHighLevel && (x.State == InstrumentState.ReachedLevel || x.State == InstrumentState.TradingNearLevel)))
                notification.LastHigh = currDate;

            if (signals.Any(x => !x.IsHighLevel && (x.State == InstrumentState.ReachedLevel || x.State == InstrumentState.TradingNearLevel)))
                notification.LastLow = currDate;

            await _notificaitonRepository.AddOrUpdateAsync(notification);
        }
    }
}
