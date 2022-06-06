using Microsoft.Extensions.Logging;
using RCalcTelegramWebhook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RCalcTelegramWebhook.Services
{
    public class UpdateService : IUpdateService
    {
        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService(IBotService botService, ILogger<UpdateService> logger)
        {
            _botService = botService;
            _logger = logger;
        }

        public async Task EchoAsync(Update update)
        {
            var message = update.Message;

            var options = new StringBuilder();
            options.AppendLine("4 Band Resistor Value Calculator:");
            options.AppendLine("----------------------------------");
            options.AppendLine("Please input the below values with comma separator");
            options.AppendLine("e.g: 2,0,0,1");
            options.AppendLine("----------------------------------");
            options.AppendLine("<b>0:   Black</b>");
            options.AppendLine("<b>1:   Brown</b>");
            options.AppendLine("<b>2:   Red</b>");
            options.AppendLine("<b>3:   Orange</b>");
            options.AppendLine("<b>4:   Yellow</b>");
            options.AppendLine("<b>5:   Green</b>");
            options.AppendLine("<b>6:   Blue</b>");
            options.AppendLine("<b>7:   Violet</b>");
            options.AppendLine("<b>8:   Grey</b>");
            options.AppendLine("<b>9:   White</b>");
            options.AppendLine("<b>10:  Gold</b>");
            options.AppendLine("<b>11:  Silver</b>");

            var invalidInput = $"Sorry I am an automated system and didn't understand your reply.\n{options.ToString()}";

            if (update.Type != UpdateType.Message)
            {
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, $"<i><b>Sorry i am unable to understand your query?</b></i>\n{options.ToString()}", ParseMode.Html);
                return;
            }

            try
            {
                if (message.Type != MessageType.Text)
                {
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, $"<i><b>Sorry i am unable to understand your query?</b></i>\n{options.ToString()}", ParseMode.Html);
                    return;
                }

                var userInput = message.Text.ToLower().Trim();

                if (userInput == "/start")
                {
                    var name = message.Chat.FirstName;
                    if (!string.IsNullOrEmpty(message.Chat.LastName))
                        name += $" {message.Chat.LastName}";

                    var welcomeMessage = new StringBuilder();
                    welcomeMessage.AppendLine($"Hello {name}\n");
                    welcomeMessage.AppendLine(options.ToString());
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, welcomeMessage.ToString(), ParseMode.Html);
                }
                else
                {
                    CalculateValue(userInput, message);
                }
            }
            catch (Exception ex)
            {
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, ex.Message, ParseMode.Html);
            }
        }

        private async void CalculateValue(string colorCodes, Message message)
        {
            try
            {
                var decodedColorCodes = colorCodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (decodedColorCodes.Length != 4)
                {
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, "Provided input is not valid!", ParseMode.Html);
                    return;
                }

                var bandColors = new List<BandColorModel>
                {
                    new BandColorModel { Name = "Black", Code = "0", Value = 0, HexCode = "#000" },
                    new BandColorModel { Name = "Brown", Code = "1", Value = 1, HexCode = "#964b00", Tolerance = "+/- 1%" },
                    new BandColorModel { Name = "Red", Code = "2", Value = 2, HexCode = "#ff0000", Tolerance = "+/- 2%" },
                    new BandColorModel { Name = "Orange", Code = "3", Value = 3, HexCode = "#ffa500" },
                    new BandColorModel { Name = "Yellow", Code = "4", Value = 4, HexCode = "#ffff00" },
                    new BandColorModel { Name = "Green", Code = "5", Value = 5, HexCode = "#9acd32", Tolerance = "+/- 0.5%" },
                    new BandColorModel { Name = "Blue", Code = "6", Value = 6, HexCode = "#6495ed", Tolerance = "+/- 0.25%" },
                    new BandColorModel { Name = "Violet", Code = "7", Value = 7, HexCode = "#9400d3", Tolerance = "+/- 0.1%" },
                    new BandColorModel { Name = "Grey", Code = "8", Value = 8, HexCode = "#a0a0a0", Tolerance = "+/- 0.05%" },
                    new BandColorModel { Name = "White", Code = "9", Value = 9, HexCode = "#fff" },
                    new BandColorModel { Name = "Gold", Code = "10", Value = 10, HexCode = "#cfb53b", Tolerance = "+/- 5%" },
                    new BandColorModel { Name = "Silver", Code = "11", Value = 11, HexCode = "#c0c0c0", Tolerance = "+/- 10%" }
                };

                var bandTolerance = new Dictionary<string, float> {
                    { "None", 0f },
                    { "Silver", 0.1f },
                    { "Gold", 0.05f },
                    { "Brown", 0.01f },
                    { "Red", 0.02f },
                    { "Yellow", 0.04f },
                    { "Green", 0.005f },
                    { "Blue", 0.0025f },
                    { "Violet", 0.0010f },
                    { "Gray", 0.0005f }
                };

                var bandOneColor = bandColors.FirstOrDefault(x => x.Code == decodedColorCodes[0]);
                var bandTwoColor = bandColors.FirstOrDefault(x => x.Code == decodedColorCodes[1]);
                var bandThreeColor = bandColors.FirstOrDefault(x => x.Code == decodedColorCodes[2]);
                var bandFourColor = bandColors.FirstOrDefault(x => x.Code == decodedColorCodes[3]);

                if (bandOneColor == null || bandTwoColor == null || bandThreeColor == null || bandFourColor == null)
                {
                    await _botService.Client.SendTextMessageAsync(message.Chat.Id, "One among the color code provided is invalid!", ParseMode.Html);
                    return;
                }

                var unit = "Ω";
                if (bandThreeColor.Value > 1 && bandThreeColor.Value < 5)
                    unit = "KΩ";
                if (bandThreeColor.Value >= 5 && bandThreeColor.Value < 8)
                    unit = "MΩ";
                if (bandThreeColor.Value >= 8)
                    unit = "GΩ";

                var resistorColor = $"{bandOneColor.Name},{bandTwoColor.Name},{bandThreeColor.Name},{bandFourColor.Name}";
                var baseValue = Convert.ToInt32($"{bandOneColor.Value}{bandTwoColor.Value}");
                var resistorValue = baseValue * Math.Pow(10, bandThreeColor.Value);

                await _botService.Client.SendTextMessageAsync(message.Chat.Id, $"{resistorColor}\nThe value is {resistorValue:N0} {unit} ({bandFourColor.Tolerance})", ParseMode.Html);
            }
            catch (Exception ex)
            {
                await _botService.Client.SendTextMessageAsync(message.Chat.Id, ex.Message, ParseMode.Html);
            }
        }
    }
}
