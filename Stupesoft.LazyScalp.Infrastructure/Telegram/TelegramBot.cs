using Microsoft.Extensions.Options;
using Stupesoft.LazeScallp.Application.Abstractions;
using Stupesoft.LazeScallp.Application.Configurations;
using Stupesoft.LazeScallp.Application.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Stupesoft.LazyScalp.Infrastructure.Telegram;

public class TelegramBot : INotification
{
    private readonly ITelegramBotClient _client;
    private readonly IOptions<TelegramBotOptions> _telegramBotOptions;

    public TelegramBot(ITelegarmClientBotFactory telegarmClientBotFactory, IOptions<TelegramBotOptions> telegramBotOptions)
    {
        _client = telegarmClientBotFactory.Create();
        _telegramBotOptions = telegramBotOptions;
    }

    public async Task SendAsync(NotificationMessage message)
    {
        InputOnlineFile file = new InputOnlineFile(new MemoryStream(message.Image));
        await _client.SendPhotoAsync(new ChatId(
            _telegramBotOptions.Value.ChatId!),
            file,
            message.Text,
            parseMode: ParseMode.Markdown);
    }

    public void RemoveAllMessage(int maxId = 9999)
    {
        for (int i = 0; i < maxId; i++)
        {
            _client.DeleteMessageAsync(_telegramBotOptions.Value.ChatId!, i);
        }
    }
}
