using Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace TradingViewBot;

public class TelegramBot : INotification
{
    private readonly TelegramBotClient _client;
    private readonly string _chatId;

    public TelegramBot(TelegramBotClient client, string chatId)
    {
        _client = client;
        _chatId = chatId;
    }

    public async Task SendAsync(TelegramMessage message)
    {
        InputOnlineFile file = new InputOnlineFile(new MemoryStream(message.Image));
        await _client.SendPhotoAsync(new ChatId(_chatId), file, message.Text);
    }

    public void RemoveAllMessage(int maxId = 9999)
    {
        for (int i = 0; i < maxId; i++)
        {
            _client.DeleteMessageAsync(_chatId, i);
        }
    }
}
