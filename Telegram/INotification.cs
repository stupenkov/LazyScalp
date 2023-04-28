using SkiaSharp;
using Telegram;
using static System.Net.Mime.MediaTypeNames;

namespace TradingViewBot
{
    public interface INotification
    {
        Task SendAsync(TelegramMessage message);
    }
}
