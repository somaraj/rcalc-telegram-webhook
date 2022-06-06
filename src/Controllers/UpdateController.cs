using Microsoft.AspNetCore.Mvc;
using RCalcTelegramWebhook.Services;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace RCalcTelegramWebhook.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/[controller]")]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        [HttpGet]
        public string Status()
        {
            return $"API is up and running...";
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            await _updateService.EchoAsync(update);
            return Ok();
        }
    }
}
