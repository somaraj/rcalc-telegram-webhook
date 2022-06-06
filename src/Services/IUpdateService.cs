using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RCalcTelegramWebhook.Services
{
    public interface IUpdateService
    {
        Task EchoAsync(Update update);
    }
}
