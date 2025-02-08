using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ScheduleBot.Resources.Extensions;
using ScheduleBot.Resources.Tools;
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
    [new KeyboardButton("Сегодня"), new KeyboardButton("Завтра")],
    [new KeyboardButton("Понедельник"), new KeyboardButton("Вторник"), new KeyboardButton("Среда")],
    [new KeyboardButton("Четверг"), new KeyboardButton("Пятница"), new KeyboardButton("Суббота")],
    [new KeyboardButton("⚙ Настройки"), new KeyboardButton("❓ Справка")]
});
var settingsKeyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
{
    [InlineKeyboardButton.WithCallbackData("⏲ Отправка по расписанию", "alarm_settings")],
    [InlineKeyboardButton.WithCallbackData("❌ Отменить регистрацию", "cancel_registration")]
});

bot.StartReceiving(async (botClient, update, token) =>
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
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
                    
                    var user = users.FirstOrDefault(x => x.Id == chat.Id);
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
                                    Id = chat.Id,
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
                            
                            File.WriteAllText("users.json", JsonSerializer.Serialize(users, jsonOptions));
                            logger.LogInformation("Updated users.json");
                            return;
                        
                        case "сегодня" when user is not null && user.Location == Location.Menu:
                            var response = groups.First(x => x.Name == user.Group)
                                .Lessons.GetScheduleFor(DateTime.Now.DayOfWeek, settings.SemesterStart);
        
                            await botClient.SendMessage(chat.Id, response, cancellationToken: token, replyMarkup: menuKeyboard);
                            logger.LogInformation("Schedule for today was sent");
                            break;
                        
                        case "завтра" when user is not null && user.Location == Location.Menu:
                            response = groups.First(x => x.Name == user.Group)
                                .Lessons.GetScheduleFor(DateTime.Now.AddDays(1).DayOfWeek, settings.SemesterStart, "завтра");
        
                            await botClient.SendMessage(chat.Id, response, cancellationToken: token, replyMarkup: menuKeyboard);
                            logger.LogInformation("Schedule for tomorrow was sent");
                            break;
                        
                        case "⚙ настройки" when user is not null && user.Location == Location.Menu:
                            //user.Location = Location.Settings;
                            await botClient.SendMessage(chat.Id, "Настройки", replyMarkup: settingsKeyboard, cancellationToken: token);
                            logger.LogInformation("Settings was sent");
                            
                            break;
                        
                        case "❓ справка":
                            await botClient.SendMessage(chat.Id, BotResources.HelpString + Environment.NewLine + Environment.NewLine +
                                "Info for nerds - /stats command", cancellationToken: token, replyMarkup: menuKeyboard);
                            logger.LogInformation("Help message was sent");
                            break;
        
                        case "/stats":
                            await botClient.SendMessage(chat.Id, "Assembly version: " + (version.Major == 0 ? "BETA " : "") + version + Environment.NewLine +
                                BotResources.ServerInfoString, cancellationToken: token);
                            logger.LogInformation("Stats message was sent");
                            break;
                    }
                    
                    if (user is not null && user.Location == Location.Registration)
                    {
                        if (!regexGroup.IsMatch(messageText.ToUpper()) || groups.All(x => !x.Name.Equals(messageText, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            await botClient.SendMessage(chat.Id, "Такой группы не существует, попробуйте еще раз", cancellationToken: token);
                        }
                        else
                        {
                            user.Group = regexGroup.Match(messageText.ToUpper()).Value;
                            user.Location = Location.Menu;
                            user.IsAlarmOn = false;
                
                            logger.LogInformation("User {Id} is registered with group {Group}", user.Id, user.Group);
                
                            await botClient.SendMessage(chat.Id, "Регистрация успешна. Вы теперь в меню.", 
                                replyMarkup: menuKeyboard, cancellationToken: token);
                            await botClient.SendMessage(chat.Id, BotResources.HelpString, cancellationToken: token);
                        }
                        return;
                    }
                    
                    if (user is not null && user.Location == Location.Menu && Schedule.DaysOfWeek.Contains(messageText.ToLower()))
                    {
                        var response = groups.First(x => x.Name == user.Group)
                            .Lessons.GetScheduleFor((DayOfWeek)Schedule.DaysOfWeek.ToList().IndexOf(messageText.ToLower()), 
                                settings.SemesterStart, 
                                Schedule.DaysOfWeekVerbal[Schedule.DaysOfWeek.ToList().IndexOf(messageText.ToLower())]);
                        
                        await botClient.SendMessage(chat.Id, response, cancellationToken: token, replyMarkup: menuKeyboard);
                        
                        logger.LogInformation("Schedule for {DoW} was sent", 
                            Enum.GetName((DayOfWeek)Schedule.DaysOfWeek.ToList().IndexOf(messageText.ToLower())));
                    }
            
                    if (user is null)
                        return;
                    
                    switch (user.Location)
                    {
                        case Location.AlarmSet when !regexTime.IsMatch(messageText.ToLower()):
                            await botClient.SendMessage(chat.Id, "Время введено неверно, попробуйте еще раз", cancellationToken: token);
                            break;
                
                        case Location.AlarmSet:
                            user.IsAlarmOn = true;
                            user.AlarmTime = TimeOnly.Parse(messageText);
                            user.Location = Location.Menu;
                            await botClient.SendMessage(chat.Id, $"Расписание установлено на {user.AlarmTime:HH:mm}", cancellationToken: token);
                            logger.LogInformation("User {Id} set alarm for {Time}", user.Id, user.AlarmTime.ToString("HH:mm"));
                            break;
                    }
                    
                    break;
                
                case UpdateType.CallbackQuery:
                    var callbackQuery = update.CallbackQuery;
                    if (callbackQuery is null)
                        return;
        
                    var callbackQueryData = callbackQuery.Data;
                    var messageCallback = callbackQuery.Message;
                    if (messageCallback is null)
                        return;
        
                    logger.LogInformation("New callback query from {Username} ({Id}): {Data}", callbackQuery.From.Username,
                        callbackQuery.From.Id, callbackQueryData);
                    
                    var userCallback = users.FirstOrDefault(x => x.Id == callbackQuery.From.Id);
                    if (userCallback == null)
                    {
                        await botClient.SendMessage(messageCallback.Chat.Id, "Ваш ID не зарегистрирован. Чтобы зарегистрироваться напишите /start", 
                            replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                        return;
                    }
                    
                    switch (callbackQueryData)
                    {
                        case "alarm_settings":
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            //userCallback.Location = Location.Alarm;
                            var alarmKeyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                            {
                                [InlineKeyboardButton.WithCallbackData("⬅ Назад", "alarm_back")],
                                [InlineKeyboardButton.WithCallbackData("✔️ Включить расписание", "enable_alarm")], 
                                [InlineKeyboardButton.WithCallbackData("❌ Отключить расписание", "disable_alarm")]
                            });
                            
                            await botClient.EditMessageText(messageCallback.Chat.Id, messageCallback.MessageId, 
                                userCallback.IsAlarmOn ? $"Расписание установлено на {userCallback.AlarmTime:HH:mm}" : "Расписание отключено", 
                                replyMarkup: alarmKeyboard, cancellationToken: token);
                            
                            logger.LogInformation("Alarm settings was sent");
                            
                            break;
                        
                        case "cancel_registration":
                            users.Remove(userCallback);
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            await botClient.SendMessage(messageCallback.Chat.Id, 
                                "Ваша регистрация была отменена. Чтобы продолжить пользоваться ботом, необходимо заново зарегистрироваться, отправив /start", 
                                cancellationToken: token, replyMarkup: new ReplyKeyboardRemove());
                            
                            logger.LogInformation("Registration was canceled, user deleted");
                            break;
                        
                        case "alarm_back":
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            await botClient.EditMessageText(messageCallback.Chat.Id, messageCallback.MessageId, 
                                "Настройки", replyMarkup: settingsKeyboard, cancellationToken: token);
                            logger.LogInformation("Alarm back button was pressed");
                            break;
                        
                        case "enable_alarm":
                            userCallback.Location = Location.AlarmSet;
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            var backKeyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                            {
                                [InlineKeyboardButton.WithCallbackData("⬅ Назад", "alarm_set_back")]
                            });
                            await botClient.EditMessageText(messageCallback.Chat.Id, messageCallback.MessageId, 
                                "Отправьте время в формате ЧЧ:ММ (Например: 08:00)", cancellationToken: token, replyMarkup: backKeyboard);
                            logger.LogInformation("Alarm enable button was pressed");
                            break;
                        
                        case "disable_alarm":
                            userCallback.Location = Location.Menu;
                            userCallback.IsAlarmOn = false;
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            await botClient.EditMessageText(messageCallback.Chat.Id, messageCallback.MessageId, 
                                "Расписание отключено", cancellationToken: token);
                            logger.LogInformation("Alarm disable button was pressed");
                            break;
                        
                        case "alarm_set_back":
                            userCallback.Location = Location.Alarm;
                            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
                            
                            await botClient.EditMessageText(messageCallback.Chat.Id, messageCallback.MessageId, 
                                userCallback.IsAlarmOn ? $"Расписание установлено на {userCallback.AlarmTime:HH:mm}" : "Расписание отключено", 
                                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                                {
                                    [InlineKeyboardButton.WithCallbackData("⬅ Назад", "alarm_back")],
                                    [InlineKeyboardButton.WithCallbackData("✔️ Включить расписание", "enable_alarm")], 
                                    [InlineKeyboardButton.WithCallbackData("❌ Отключить расписание", "disable_alarm")]
                                }), cancellationToken: token);
                            
                            logger.LogInformation("Alarm settings was sent");
                            break;
                    }
        
                    break;
            }
            
            var usersJson = JsonSerializer.Serialize(users, jsonOptions);
            File.WriteAllText("users.json", usersJson);
            logger.LogInformation("Updated users.json");
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

var timer = new Timer(_ =>
{
    foreach (var user in users.Where(x =>
                 x.IsAlarmOn && x.AlarmTime == new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute)))
    {
        var group = groups.First(x => x.Name == user.Group);
        var response = group.Lessons.GetScheduleFor(DateTime.Now.DayOfWeek, settings.SemesterStart);
        bot.SendMessage(user.Id!, response);
    }
}, null, 0, 60000);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    logger.LogInformation("Shutting down...");
    timer.Dispose();
    tokenSource.Cancel();
};

AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    logger.LogCritical(e.ExceptionObject as Exception, "Unhandled exception was occurred");
    Process.Start(new ProcessStartInfo { FileName = "dotnet", Arguments = Assembly.GetExecutingAssembly().Location });
    Environment.Exit(-1);
};

logger.LogInformation("Startup successful, waiting for messages...");
await Task.Delay(-1);