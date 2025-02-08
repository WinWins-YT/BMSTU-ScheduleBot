using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ScheduleBot.Resources.Extensions;
using ScheduleBot.TelegramBot.Converters;
using ScheduleBot.TelegramBot.Models;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Group = ScheduleBot.Resources.Models.Group;
using Location = ScheduleBot.Resources.Enums.Location;
using User = ScheduleBot.Resources.Models.User;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

var appRoot = AppDomain.CurrentDomain.BaseDirectory;
Random rnd = new();
var version = Assembly.GetExecutingAssembly().GetName().Version!;
var tokenSource = new CancellationTokenSource();
var jsonOptions = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters = { new JsonDateTimeConverter(), new JsonTimeOnlyConverter() }
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(Path.Combine(appRoot, "logs", $"log_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.log"))
    .WriteTo.Console()
    .CreateLogger();

var logger = LoggerFactory.Create(x => x.AddSerilog(dispose: true)).CreateLogger<Program>();

if (!File.Exists("schedule.json"))
{
    logger.LogCritical(
        "Couldn't find schedule.json file. See README file here: https://github.com/WinWins-YT/BMSTU-ScheduleBot");
    return;
}

if (!File.Exists("settings.json"))
{
    logger.LogCritical(
        "Couldn't find settings.json file. See README file here: https://github.com/WinWins-YT/BMSTU-ScheduleBot");
    return;
}

logger.LogInformation("Loading settings.json...");
var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"), jsonOptions);
if (settings is null)
{
    logger.LogCritical("Settings object loaded from settings.json is null. Check this file");
    return;
}

logger.LogInformation("Loading schedule.json...");
var groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText("schedule.json"), jsonOptions);
if (groups is null)
{
    logger.LogCritical("Schedule object loaded from schedule.json is null. Check this file");
    return;
}

List<User> users = [];
if (File.Exists("users.json"))
{
    logger.LogInformation("Loading users.json...");
    users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("users.json"), jsonOptions);
}
else
    logger.LogInformation("File users.json not found, skipping...");

logger.LogInformation("Initializing Telegram bot...");
var bot = new TelegramBotClient(settings.Token);
logger.LogInformation("Getting bot info...");
var me = await bot.GetMe(tokenSource.Token);

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates =
    [
        UpdateType.Message,
        UpdateType.CallbackQuery
    ],
    DropPendingUpdates = true
};

var menuKeyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
{
    
});

bot.StartReceiving(async (botClient, update, token) =>
    {
        try
        {
            var message = update.Message;
            if (message is null)
                return;

            var from = message.From ?? new Telegram.Bot.Types.User();
            var messageText = message.Text ?? "<Empty>";
            var chat = message.Chat;
            logger.LogInformation("New message from {Username} ({Id}): {Message}", from.Username, from.Id,
                messageText);

            Regex regexGroup = new(@"(ИУК([1-7]|11)|МК([1-9]|11))-\d{2,3}[БМ]?");
            Regex regexTime = new("([0-1]?[0-9]|2[0-3]):[0-5][0-9]");
            
            var user = users.FirstOrDefault(x => x.Id == from.Id);
            if (user == null && !messageText.Equals("/start", StringComparison.CurrentCultureIgnoreCase))
            {
                await botClient.SendMessage(chat.Id, "Ваш ID не зарегистрирован. Чтобы зарегистрироваться напишите /start", 
                    replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                return;
            }

            switch (messageText.ToLower())
            {
                case "/start":
                    if (user is null)
                    {
                        user = new User
                        {
                            Id = from.Id,
                            Location = Location.Registration
                        };
                        users.Add(user);
                        logger.LogInformation("Added new user with ID {Id} to list", user.Id);
                    }
                    else if (user.Location != Location.Registration)
                    {
                        await botClient.SendMessage(chat.Id,
                            $"Ваш ID уже зарегистрирован, ваша группа {user.Group}. Отменить регистрацию можно в настройках", 
                            cancellationToken: token);
                        return;
                    }

                    await botClient.SendMessage(chat.Id,
                        "Добро пожаловать в замечательного и очень хорошо работающего бота для помощи студентам, " +
                        "если вдруг че не работает, просьба сообщать @top_programer или @Methanol_dude. " +
                        "Чтобы зарегистрироваться напишите вашу группу, например ИУК1-11Б",
                        replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                    
                    logger.LogInformation("Welcome message was sent");
                    break;
                
                case "сегодня" when user is not null && user.Location == Location.Menu:
                    var response = groups.First(x => x.Name == user.Group)
                        .Lessons.GetScheduleFor(DateTime.Now.DayOfWeek, settings.SemesterStart);

                    await botClient.SendMessage(chat.Id, response, cancellationToken: token);
                    logger.LogInformation("Schedule for today was sent");
                    break;
                
                case "завтра" when user is not null && user.Location == Location.Menu:
                    response = groups.First(x => x.Name == user.Group)
                        .Lessons.GetScheduleFor(DateTime.Now.AddDays(1).DayOfWeek, settings.SemesterStart, "завтра");

                    await botClient.SendMessage(chat.Id, response, cancellationToken: token);
                    logger.LogInformation("Schedule for tomorrow was sent");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Exception occurred at update: {Message}", ex.Message);
        }
    },
    (_, exception, _) =>
    {
        switch (exception)
        {
            case ApiRequestException apiRequestException:
                logger.LogError("Telegram API request error, code: {ErrorCode} , message: {Message}",
                    apiRequestException.ErrorCode, apiRequestException.Message);
                break;

            default:
                logger.LogError("Unknown exception: {Message}", exception.Message);
                break;
        }

        return Task.CompletedTask;
    },
    receiverOptions, tokenSource.Token);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    logger.LogInformation("Shutting down...");
    //timer.Dispose();
    tokenSource.Cancel();
};

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    logger.LogCritical(e.ExceptionObject as Exception, "Unhandled exception was occurred");
    Process.Start(new ProcessStartInfo { FileName = "dotnet", Arguments = Assembly.GetExecutingAssembly().Location });
    Environment.Exit(-1);
};

logger.LogInformation("Startup successful, waiting for messages...");