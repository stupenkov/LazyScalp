using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Configurations;
using Telegram.Bot;

namespace Stupesoft.LazyScalp.Infrastructure.Telegram;

public class TelegramClientBotFactory : ITelegarmClientBotFactory
{
    private readonly IOptions<TelegramBotOptions> _options;

    public TelegramClientBotFactory(IOptions<TelegramBotOptions> options)
    {
        _options = options;
    }

    public ITelegramBotClient Create()
    {
        return new TelegramBotClient(_options.Value.Token!);
    }
}
