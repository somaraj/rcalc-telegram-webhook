using Telegram.Bot;

namespace RCalcTelegramWebhook.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
    }
}