using System;
using Telegram.Bot;

namespace RCalcTelegramWebhook.Services
{
    public class BotService : IBotService
    {
        public BotService()
        {
            var botToken = Environment.GetEnvironmentVariable("BotToken");
            Client = new TelegramBotClient(botToken);
        }

        public TelegramBotClient Client { get; }
    }
}
