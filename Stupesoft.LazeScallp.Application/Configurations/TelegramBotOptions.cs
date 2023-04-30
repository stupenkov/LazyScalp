namespace Stupesoft.LazeScallp.Application.Configurations;

public class TelegramBotOptions
{
    public const string TelegramBot = "TelegramBot";
    public string? Token { get; set; }
    public string? ChatId { get; set; }
}