using Telegram.Bot;

namespace Stupesoft.LazyScalp.Infrastructure.Telegram;
public interface ITelegarmClientBotFactory
{
    ITelegramBotClient Create();
}
