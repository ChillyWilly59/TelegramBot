using Bot.Model;
using Bot.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Example
{
    public class Program
    {
        private static readonly string BotToken = "6003015187:AAGBXwTkv8r9aFLfaVMbqrVnplpae4-_XxI";
        private static readonly string WeatherApiKey = "f0d33cd3-883b-4d94-9ab3-02dabd40b6ea";

        private static readonly Dictionary<long, UserState> UserStates = new Dictionary<long, UserState>();
        private static readonly BotDbContext _dbContext;
        private static ITelegramBotClient BotClient;
        private static ReceiverOptions ReceiverOptions;

        public static async Task Main()
        {
            BotClient = new TelegramBotClient(BotToken);
            ReceiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
                ThrowPendingUpdates = true
            };

            using var cts = new CancellationTokenSource();

            BotClient.StartReceiving(UpdateHandler, ErrorHandler, ReceiverOptions, cts.Token);

            var me = await BotClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1);
        }

        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(botClient, update.Message, cancellationToken);
                        break;
                    case UpdateType.CallbackQuery:
                        await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var user = message.From;
            var chat = message.Chat;

            switch (message.Type)
            {
                case MessageType.Text:
                    if (message.Text == "/start")
                    {
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Найти город", "button1"),
                                
                            }
                        });

                        await botClient.SendTextMessageAsync(chat.Id, "С Чего начнем!", replyMarkup: inlineKeyboard);

                        return;
                    }

                    if (UserStates.TryGetValue(user.Id, out var userState))
                    {
                        switch (userState)
                        {
                            case UserState.CityInput:
                                var enteredCity = message.Text;

                                var weatherService = new WeatherService();
                                var weatherResult = await weatherService.GetWeatherAsync(enteredCity);

                                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Найти город", "button1"),
                                        
                                    }
                                });

                                await botClient.SendTextMessageAsync(chat.Id, $"Погода в городе {enteredCity}:\n{weatherResult}", replyMarkup: inlineKeyboard);

                                UserStates.Remove(user.Id);

                                return;
                        }
                    }

                    break;
            }
        }

        private static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var user = callbackQuery.From;
            var chat = callbackQuery.Message.Chat;

            switch (callbackQuery.Data)
            {
                case "button1":
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                    UserStates[user.Id] = UserState.CityInput;

                    await botClient.SendTextMessageAsync(chat.Id, "Введите город");
                    break;
                /*case "button2":
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Город добавлен в избранное!");

                    if (UserStates.TryGetValue(user.Id, out var userState) && userState == UserState.CityInput)
                    {
                        var extractedCityName = callbackQuery.Message.Text;
                        var cityName = extractedCityName;

                        var favoriteCitiesServices = new FavoriteCitiesServices(_dbContext);

                        favoriteCitiesServices.AddFavoriteCity(user.Id, cityName);

                        UserStates.Remove(user.Id);

                        await botClient.SendTextMessageAsync(chat.Id, $"Город {cityName} добавлен в избранное!");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "Ошибка: Некорректное состояние для добавления в избранное.");
                        Console.WriteLine($"");
                    }

                    break;*/
            }
        }

        public static Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}

