using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using ScheduleBot.Resources.Enums;
using ScheduleBot.Resources.Extensions;
using ScheduleBot.Resources.Tools;
using ScheduleBot.VkBot.Converters;
using ScheduleBot.VkBot.Models;
using Serilog;
using VkNet;
using VkNet.Enums.StringEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils.BotsLongPool;
using Group = ScheduleBot.Resources.Models.Group;
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
    logger.LogCritical("Couldn't find schedule.json file. See README file here: https://github.com/WinWins-YT/BMSTU-ScheduleBot");
    return;
}

if (!File.Exists("settings.json"))
{
    logger.LogCritical("Couldn't find settings.json file. See README file here: https://github.com/WinWins-YT/BMSTU-ScheduleBot");
    return;
}

logger.LogInformation("Loading settings.json...");
var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"), jsonOptions);
if (settings is null)
{
    logger.LogCritical("Setttings object loaded from settings.json is null. Check this file.");
    return;
}

logger.LogInformation("Loading schedule.json...");
var groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText("schedule.json"), jsonOptions);
if (groups is null)
{
    logger.LogCritical("Schedule object loaded from schedule.json is null. Check this file.");
    return;
}

List<User> users = new();
if (File.Exists("users.json"))
{
    logger.LogInformation("Loading users.json...");
    users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText("users.json"), jsonOptions);
}
else
    logger.LogInformation("File users.json not found, skipping...");

var vk = new VkApi();

logger.LogInformation("Authorizing...");
await vk.AuthorizeAsync(new ApiAuthParams
{
    AccessToken = settings.Token
}, tokenSource.Token);

logger.LogInformation("Getting ID of {groupUrl}...", settings.GroupUrl);
var groupIdObject = await vk.Utils.ResolveScreenNameAsync(settings.GroupUrl.Substring(settings.GroupUrl.LastIndexOf('/') + 1));
logger.LogInformation("ID resolved, it is {id}", groupIdObject.Id);

var menuKeyboard = new KeyboardBuilder()
    .AddButton("Сегодня", "", KeyboardButtonColor.Primary)
    .AddButton("Завтра", "", KeyboardButtonColor.Primary)
    .AddLine()
    .AddButton("Понедельник", "")
    .AddButton("Вторник", "")
    .AddButton("Среда", "")
    .AddLine()
    .AddButton("Четверг", "")
    .AddButton("Пятница", "")
    .AddButton("Суббота", "")
    .AddLine()
    .AddButton("⚙ Настройки", "", KeyboardButtonColor.Positive)
    .AddButton("❓ Справка", "", KeyboardButtonColor.Positive)
    .Build();

var longPool = new BotsLongPoolUpdatesHandler(new BotsLongPoolUpdatesHandlerParams(vk, Convert.ToUInt64(groupIdObject.Id))
{
    GetPause = () => false,
    
    OnException = ex =>
    {
        logger.LogError(ex, "Exception occurred that can cause malfunctions");
    },
    
    OnWarn = ex =>
    {
        switch (ex)
        {
            case PublicServerErrorException:
            case HttpRequestException:
            case SocketException:
                logger.LogWarning(ex, "VK servers are down");
                break;
            
            default:
                logger.LogWarning(ex, "Exception occured that not causing malfunctions");
                break;
        }
    },
    
    OnUpdates = e =>
    {
        var updates = new List<GroupUpdate>();

        foreach (var update in e.Updates)
        {
            if (update.Update != null)
            {
                updates.Add(update.Update);
                continue;
            }
            
            if (update.Exception == null)
                continue;
            
            logger.LogError("JSON serialization failed");
        }

        if (!updates.Any())
            return;

        var newMessages = updates.Where(x => x.Instance is MessageNew)
            .Select(x => x.Instance as MessageNew)
            .Select(x => x?.Message);

        foreach (var message in newMessages)
        {
            if (message == null)
                continue;
            
            if (message.PeerId >= 2000000000) return;
            
            Regex regexGroup = new(@"(ИУК[1-7]|МК[1-9])-\d{2,3}[БМ]?");
            Regex regexTime = new("([0-1]?[0-9]|2[0-3]):[0-5][0-9]");
            
            logger.LogInformation("New message from {Id}: {message}", message.PeerId, message.Text);

            var user = users.FirstOrDefault(x => x.Id == message.FromId);
            if (user == null && (message.Text.ToLower() != "начать" && message.Text.ToLower() != "start"))
            {
                vk.Messages.Send(new MessagesSendParams
                {
                    RandomId = rnd.Next(),
                    Message = "Ваш ID не зарегистрирован. Чтобы зарегистрироваться напишите Начать",
                    Keyboard = new KeyboardBuilder().Clear().Build(),
                    PeerId = message.PeerId
                });
                continue;
            }

            string response;
            
            switch (message.Text.ToLower())
            {
                case "начать":
                case "start":
                    if (user == null)
                    {
                        user = new User
                        {
                            Id = message.FromId,
                            Location = Location.Registration
                        };
                        users.Add(user);
                        logger.LogInformation("Added new user with ID {id} to list", user.Id);
                    }
                    else
                    {
                        user.Location = Location.Registration;
                    }
                    
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message =
                            "Добро пожаловать в замечательного и очень хорошо работающего бота для помощи студентам, " +
                            "если вдруг че не работает, просьба сообщать @top_programer или @sanekmethanol. " +
                            "Чтобы зарегистрироваться напишите вашу группу, например ИУК1-11Б",
                        Keyboard = new KeyboardBuilder().Clear().Build(),
                        PeerId = message.PeerId
                    });
                    
                    logger.LogInformation("Welcome message was sent");
                    continue;
                
                case "сегодня" when user is not null && user.Location == Location.Menu:
                    response = groups.First(x => x.Name == user.Group)
                        .Lessons.GetScheduleFor(DateTime.Now.DayOfWeek, settings.SemesterStart);

                    vk.Messages.Send(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = response,
                        Keyboard = menuKeyboard,
                        PeerId = message.PeerId
                    });
                    logger.LogInformation("Schedule for today was sent");
                    break;
                
                case "завтра" when user is not null && user.Location == Location.Menu:
                    response = groups.First(x => x.Name == user.Group)
                        .Lessons.GetScheduleFor(DateTime.Now.AddDays(1).DayOfWeek, settings.SemesterStart, "завтра");

                    vk.Messages.Send(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = response,
                        Keyboard = menuKeyboard,
                        PeerId = message.PeerId
                    });
                    logger.LogInformation("Schedule for tomorrow was sent");
                    break;
                
                case "⚙ настройки" when user is not null && user.Location == Location.Menu:
                    user.Location = Location.Settings;
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Настройки",
                        Keyboard = new KeyboardBuilder()
                            .AddButton("⬅ Назад", "")
                            .AddLine()
                            .AddButton("⏲ Отправка по расписанию", "",
                                user.IsAlarmOn ? KeyboardButtonColor.Positive : KeyboardButtonColor.Default)
                            .AddLine()
                            .AddButton("❌ Отменить регистрацию", "", KeyboardButtonColor.Negative)
                            .Build(),
                        PeerId = message.PeerId
                    });
                    
                    break;
                
                case "❌ отменить регистрацию" when user is not null && user.Location == Location.Settings:
                    users.Remove(user);
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message =
                            "Ваша регистрация была отменена. Чтобы продолжить пользоваться ботом, необходимо заново зарегистрироваться, отправив слово Начать",
                        Keyboard = new KeyboardBuilder().Clear().Build(),
                        PeerId = message.PeerId
                    });
                    
                    break;
                
                case "⏲ отправка по расписанию" when user is not null && user.Location == Location.Settings:
                    user.Location = Location.Alarm;
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = user.IsAlarmOn ? $"Расписание установлено на {user.AlarmTime:HH:mm}" : "Расписание отключено",
                        Keyboard = new KeyboardBuilder()
                            .AddButton("⬅ Назад", "")
                            .AddLine()
                            .AddButton("✔️ Включить расписание", "")
                            .AddLine()
                            .AddButton("❌ Отключить расписание", "")
                            .Build(),
                        PeerId = message.PeerId
                    });
                    break;
                
                case "✔️ включить расписание" when user is not null && user.Location == Location.Alarm:
                    user.Location = Location.AlarmSet;
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Отправьте время в формате ЧЧ:ММ (08:00)",
                        Keyboard = new KeyboardBuilder()
                            .AddButton("⬅ Назад", "")
                            .Build(),
                        PeerId = message.PeerId
                    });
                    continue;
                
                case "❌ отключить расписание" when user is not null && user.Location == Location.Alarm:
                    user.Location = Location.Menu;
                    user.IsAlarmOn = false;
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Расписание отключено",
                        Keyboard = menuKeyboard,
                        PeerId = message.PeerId
                    });
                    logger.LogInformation("User {id} disabled alarm", user.Id);
                    break;
                
                case "⬅ назад" when user is not null:
                    switch (user.Location)
                    {
                        case Location.Settings:
                            user.Location = Location.Menu;
                            vk.Messages.SendAsync(new MessagesSendParams
                            {
                                RandomId = rnd.Next(),
                                Message = "Меню",
                                Keyboard = menuKeyboard,
                                PeerId = message.PeerId
                            });
                            break;
                        case Location.AlarmSet:
                            user.Location = Location.Alarm;
                            vk.Messages.SendAsync(new MessagesSendParams
                            {
                                RandomId = rnd.Next(),
                                Message = user.IsAlarmOn ? $"Расписание установлено на {user.AlarmTime:HH:mm}" : "Расписание отключено",
                                Keyboard = new KeyboardBuilder()
                                    .AddButton("⬅ Назад", "")
                                    .AddLine()
                                    .AddButton("✔️ Включить расписание", "")
                                    .AddLine()
                                    .AddButton("❌ Отключить расписание", "")
                                    .Build(),
                                PeerId = message.PeerId
                            });
                            
                            break;
                        case Location.Alarm:
                            user.Location = Location.Settings;
                            vk.Messages.SendAsync(new MessagesSendParams
                            {
                                RandomId = rnd.Next(),
                                Message = "Настройки",
                                Keyboard = new KeyboardBuilder()
                                    .AddButton("⬅ Назад", "")
                                    .AddLine()
                                    .AddButton("⏲ Отправка по расписанию", "",
                                        user.IsAlarmOn ? KeyboardButtonColor.Positive : KeyboardButtonColor.Default)
                                    .AddLine()
                                    .AddButton("❌ Отменить регистрацию", "", KeyboardButtonColor.Negative)
                                    .Build(),
                                PeerId = message.PeerId
                            });
                            break;
                        case Location.Menu:
                            vk.Messages.SendAsync(new MessagesSendParams
                            {
                                RandomId = Environment.TickCount,
                                Message = "Ты дурак?",
                                Keyboard = menuKeyboard,
                                PeerId = message.PeerId
                            });
                            break;
                    }

                    break;
                
                case "❓ справка":
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "❗ВНИМАНИЕ❗\n\n" +
                                  "Расписание может плохо спарситься или же измениться в течение семестра. Бот не несет ответственности за такие ошибки,\n" +
                                  "невозможно что - то сделать без косяков(надеемся на взаимопонимание).Бот призван облегчить жизнь студентам,\n" +
                                  "поэтому при обнаружении ошибок – сообщать https://vk.com/top_programer или https://vk.com/sanekmethanol\n\n" +
                                  "⚠ Инструкция:\n\n" +
                                  "📌 Есть кнопочка с расписанием, а есть кнопочка с настройками\n" +
                                  "📌Если нажать кнопочку с настройками, откроются настройки\n" +
                                  "📌Если нажать кнопочку с расписанием, будет выведено расписание (ШОК!)\n" +
                                  "📌Если написать день недели, например Понедельник, то будет выведено расписание на этот день недели.\n" +
                                  "🚽Сделано WinWins и чуть-чуть Methanol на .NET 6.0.8 и C#\n" +
                                  "Version: " + (version.Major == 0 ? "BETA " : "") + version,
                        PeerId = message.PeerId
                    });
                    break;
            }

            if (user is not null && user.Location == Location.Registration)
            {
                if (!regexGroup.IsMatch(message.Text.ToUpper()) || groups.All(x => x.Name != message.Text.ToUpper()))
                {
                    vk.Messages.Send(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Такой группы не существует, попробуйте еще раз",
                        PeerId = message.PeerId
                    });
                }
                else
                {
                    user.Group = regexGroup.Match(message.Text.ToUpper()).Value;
                    user.Location = Location.Menu;
                    user.IsAlarmOn = false;
                
                    logger.LogInformation("User {id} is registered with group {group}", user.Id, user.Group);
                
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Регистрация успешна. Вы теперь в меню.",
                        Keyboard = menuKeyboard,
                        PeerId = message.PeerId
                    });
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "❗ВНИМАНИЕ❗\n\n" +
                                  "Расписание может плохо спарситься или же измениться в течение семестра. Бот не несет ответственности за такие ошибки, " +
                                  "невозможно что-то сделать без косяков(надеемся на взаимопонимание). Бот призван облегчить жизнь студентам, " +
                                  "поэтому при обнаружении ошибок – сообщать https://vk.com/top_programer или https://vk.com/sanekmethanol\n\n" +
                                  "⚠ Инструкция:\n\n" +
                                  "📌 Есть кнопочка с расписанием, а есть кнопочка с настройками\n" +
                                  "📌Если нажать кнопочку с настройками, откроются настройки\n" +
                                  "📌Если нажать кнопочку с расписанием, будет выведено расписание (ШОК!)\n" +
                                  "📌Если написать день недели, например Понедельник, то будет выведено расписание на этот день недели.\n" +
                                  $"🚽Сделано WinWins и чуть-чуть Methanol на {RuntimeInformation.FrameworkDescription} и C#\n" +
                                  "Version: " + (version.Major == 0 ? "BETA " : "") + version,
                        PeerId = message.PeerId
                    });
                }
                continue;
            }

            if (user is not null && user.Location == Location.Menu && Schedule.DaysOfWeek.Contains(message.Text.ToLower()))
            {
                response = groups.First(x => x.Name == user.Group)
                    .Lessons.GetScheduleFor((DayOfWeek)Schedule.DaysOfWeek.ToList().IndexOf(message.Text.ToLower()), 
                        settings.SemesterStart, 
                        Schedule.DaysOfWeekVerbal[Schedule.DaysOfWeek.ToList().IndexOf(message.Text.ToLower())]);
                
                vk.Messages.Send(new MessagesSendParams
                {
                    RandomId = rnd.Next(),
                    Message = response,
                    Keyboard = menuKeyboard,
                    PeerId = message.PeerId
                });
                logger.LogInformation("Schedule for {dow} was sent", 
                    Enum.GetName((DayOfWeek)Schedule.DaysOfWeek.ToList().IndexOf(message.Text.ToLower())));
            }
            
            if (user is null)
                continue;

            switch (user.Location)
            {
                case Location.AlarmSet when !regexTime.IsMatch(message.Text.ToLower()):
                    vk.Messages.Send(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Время введено неверно, попробуйте еще раз",
                        PeerId = message.PeerId
                    });
                    
                    break;
                
                case Location.AlarmSet:
                    user.IsAlarmOn = true;
                    user.AlarmTime = TimeOnly.Parse(message.Text);
                    user.Location = Location.Menu;
                    vk.Messages.SendAsync(new MessagesSendParams
                    {
                        RandomId = rnd.Next(),
                        Message = "Расписание установлено",
                        Keyboard = menuKeyboard,
                        PeerId = message.PeerId
                    });
                    logger.LogInformation("User {id} set alarm for {time}", user.Id, user.AlarmTime.ToString("HH:mm"));
                    break;
            }
        }

        var usersJson = JsonSerializer.Serialize(users, jsonOptions);
        File.WriteAllText("users.json", usersJson);
        logger.LogInformation("Updated users.json");
    }
});

var timer = new Timer(_ =>
{
    foreach (var user in users.Where(x =>
                 x.IsAlarmOn && x.AlarmTime == new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute)))
    {
        var group = groups.First(x => x.Name == user.Group);
        var response = group.Lessons.GetScheduleFor(DateTime.Now.DayOfWeek, settings.SemesterStart);

        vk.Messages.Send(new MessagesSendParams
        {
            RandomId = rnd.Next(),
            Message = response,
            Keyboard = menuKeyboard,
            PeerId = user.Id
        });
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

await longPool.RunAsync(tokenSource.Token);