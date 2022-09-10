using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkBotFramework;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using ScheduleToJSON;
using VkNet.Enums.SafetyEnums;
using Group = ScheduleToJSON.Group;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;
using User = ScheduleBot.User;
using ScheduleBot;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

if (!IsUnix())
{
    Console.InputEncoding = Encoding.Unicode;
    Console.OutputEncoding = Encoding.Unicode;
}
Version version = new(0, 4, 173);

ServiceCollection services = new();
services.AddSingleton(new HttpClient() { Timeout = TimeSpan.FromSeconds(300) });
string AppRoot = AppDomain.CurrentDomain.BaseDirectory;
CancellationTokenSource tokenSource = new();
List<User> users = new();
Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(AppRoot + "settings.json"));
JsonSerializerOptions usersJsonOptions = new JsonSerializerOptions()
{
    WriteIndented = true,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters =
    {
        new JsonTimeOnlyConverter()
    }
};
if (File.Exists(AppRoot + "users.json")) users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(AppRoot + "users.json"), usersJsonOptions);

VkBot bot = new(settings.Token, settings.GroupUrl, services);
List<Group> groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText(AppRoot + "schedule.json"));
//await bot.Api.Groups.EnableOnlineAsync(Convert.ToUInt64(bot.GroupId));
Console.WriteLine("Initialized");

MessageKeyboard menuKeyboard = new KeyboardBuilder()
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

async void Bot_OnMessageReceived(object? sender, VkBotFramework.Models.MessageReceivedEventArgs e)
{
    try
    {
        if (e.Message.PeerId >= 2000000000) return;
        VkBot instance = sender as VkBot;
        Regex regexGroup = new(@"(ИУК[1-7]|МК[1-9])-\d\d\d?(Б|М)?");
        Regex regexTime = new("([0-1]?[0-9]|2[0-3]):[0-5][0-9]");
        Console.WriteLine($"New message: {e.Message.Text}");
        if (e.Message.Text.ToLower() == "start" || e.Message.Text.ToLower() == "начать")
        {
            var server = instance.Api.Photo.GetMessagesUploadServer(instance.GroupId);
            var http = new HttpClient();
            WebClient web = new();
            string responseStr = Encoding.ASCII.GetString(web.UploadFile(server.UploadUrl, AppRoot + "ZuevFace.png"));
            var photo = await instance.Api.Photo.SaveMessagesPhotoAsync(responseStr);
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Attachments = new MediaAttachment[]
                {
                photo.First()
                },
                Keyboard = new KeyboardBuilder().SetOneTime()
                    .AddButton("Регистрация", "", KeyboardButtonColor.Primary)
                    .Build(),
                Message = "Добро пожаловать в замечательного и очень хорошо работающего бота для помощи студентам. Бот пока в бета режиме, и поэтому, если вдруг че не работает, просьба сообщать @top_programer или @sanekmethanol",
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "регистрация")
        {
            if (!users.Where(x => x.Id == e.Message.FromId).Any())
            {
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "Отправьте номер группы с командой /reg, например /reg ИУК2-31Б",
                    PeerId = e.Message.PeerId
                });
            }
            else
            {
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "Вы уже были зарегистрированы. Отменить регистрацию можно в настройках",
                    Keyboard = menuKeyboard,
                    PeerId = e.Message.PeerId
                });
            }
        }
        else if (e.Message.Text.ToLower().StartsWith("/reg ") && regexGroup.IsMatch(e.Message.Text.Split(' ')[1].ToUpper()))
        {
            if (!groups.Where(x => x.Name == e.Message.Text.Split(' ')[1].ToUpper()).Any())
            {
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "Такой группы не существует",
                    PeerId = e.Message.PeerId
                });
                return;
            }
            if (!users.Where(x => x.Id == e.Message.FromId).Any())
            {
                users.Add(new User()
                {
                    Id = e.Message.FromId,
                    Group = e.Message.Text.Split(' ')[1].ToUpper(),
                    Location = ScheduleBot.Location.Menu
                });
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "Регистрация успешна. Вы теперь в меню.",
                    Keyboard = menuKeyboard,
                    PeerId = e.Message.PeerId
                });
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "❗ВНИМАНИЕ❗\n\n" +
                        "Расписание может плохо спарситься или же измениться в течение семестра.Бот не несет ответственности за такие ошибки,\n" +
                        "невозможно что - то сделать без косяков(надеемся на взаимопонимание).Бот призван облегчить жизнь студентам,\n" +
                        "поэтому при обнаружении ошибок – сообщать https://vk.com/top_programer или https://vk.com/sanekmethanol\n\n" +
                        "⚠ Инструкция:\n\n" +
                        "📌 Есть кнопочка с расписанием, а есть кнопочка с настройками\n" +
                        "📌Если нажать кнопочку с настройками, откроются настройки\n" +
                        "📌Если нажать кнопочку с расписанием, будет выведено расписание (ШОК!)\n" +
                        "📌Если написать день недели, например Понедельник, то будет выведено расписание на этот день недели.\n" +
                        "🚽Сделано WinWins и чуть-чуть Methanol на .NET 6.0.8 и C#\n" +
                        "Version: " + (version.Major == 0 ? "BETA " : "") + version.ToString(),
                    PeerId = e.Message.PeerId
                });
            }
            else
            {
                var user = users.First(x => x.Id == e.Message.FromId);
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "Ваш ID уже зарегистрирован. Ваша группа " + user.Group,
                    PeerId = e.Message.PeerId
                });
            }
        }
        else if (e.Message.Text.ToLower() == "сегодня")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            Group group = groups.First(x => x.Name == user.Group);
            DateTime semStart = new DateTime(2022, 8, 29);
            DateTime nowMonday = DateTime.Now.AddDays((DateTime.Now.DayOfWeek == 0 ? -7 : -(int)DateTime.Now.DayOfWeek) + 1);
            bool isNumeric = (nowMonday - semStart).Days / 7 % 2 == 0;
            StringBuilder sb = new();
            int dw = (int)DateTime.Now.DayOfWeek;
            if (dw == 0)
            {
                dw = 1;
                sb.AppendLine("Так как сегодня воскресенье, то расписание на понедельник");
                isNumeric = !isNumeric;
            }
            else sb.AppendLine("Расписание на сегодня");
            List<Lesson> lessons = group.Lessons.Where(x => x.DayOfWeek == (DayOfWeek)dw && (x.Type == LessonType.All || x.Type == (isNumeric ? LessonType.Numerator : LessonType.Denominator))).ToList();
            lessons = lessons.OrderBy(x => x.StartTime).ToList();
            sb.AppendLine();
            foreach (var lesson in lessons)
            {
                sb.AppendLine($"⌛ Пара {lesson.Para}: {lesson.StartTime} - {lesson.EndTime}");
                sb.AppendLine($"📚 Предмет: {lesson.Name}");
                sb.AppendLine($"🏫 Аудитория: {lesson.Location}");
                sb.AppendLine($"👨‍🏫 Препод: {lesson.Teacher}");
                sb.AppendLine();
            }
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = sb.ToString(),
                Keyboard = menuKeyboard,
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "завтра")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            int day = (int)DateTime.Now.DayOfWeek + 1;
            DateTime semStart = new(2022, 8, 29);
            DateTime nowMonday = DateTime.Now.AddDays((day == 0 ? -7 : -day) + 2);
            bool isNumeric = (nowMonday - semStart).Days / 7 % 2 == 0;
            //if (day == 1) isNumeric = !isNumeric;
            StringBuilder sb = new();
            if (day == 7)
            {
                day = 1;
                sb.AppendLine("Так как завтра воскресенье, то расписание на понедельник");
                isNumeric = !isNumeric;
            }
            else sb.AppendLine("Расписание на завтра");
            Group group = groups.First(x => x.Name == user.Group);
            List<Lesson> lessons = group.Lessons.Where(x => x.DayOfWeek == (DayOfWeek)day && (x.Type == LessonType.All || x.Type == (isNumeric ? LessonType.Numerator : LessonType.Denominator))).ToList();
            lessons = lessons.OrderBy(x => x.StartTime).ToList();
            sb.AppendLine();
            foreach (var lesson in lessons)
            {
                sb.AppendLine($"⌛ Пара {lesson.Para}: {lesson.StartTime} - {lesson.EndTime}");
                sb.AppendLine($"📚 Предмет: {lesson.Name}");
                sb.AppendLine($"🏫 Аудитория: {lesson.Location}");
                sb.AppendLine($"👨‍🏫 Препод: {lesson.Teacher}");
                sb.AppendLine();
            }
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = sb.ToString(),
                Keyboard = menuKeyboard,
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "⚙ настройки")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            user.Location = ScheduleBot.Location.Settings;
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Настройки",
                Keyboard = new KeyboardBuilder()
                    .AddButton("⬅ Назад", "")
                    .AddLine()
                    .AddButton("⏲ Отправка по расписанию", "", user.IsAlarmOn ? KeyboardButtonColor.Positive : KeyboardButtonColor.Default)
                    .AddLine()
                    .AddButton("❌ Отменить регистрацию", "", KeyboardButtonColor.Negative)
                    .Build(),
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "❌ отменить регистрацию")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            users.Remove(user);
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Ваша регистрация была отменена. Чтобы продолжить пользоваться ботом, необходимо заново зарегистрироваться, отправив слово Начать",
                Keyboard = new KeyboardBuilder().Clear().Build(),
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "⏲ отправка по расписанию")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Чтобы включить отправку расписания в необходимое время, отправьте его в формате /alarm ЧЧ:ММ, либо воспользуйтесь клавиатурой",
                Keyboard = new KeyboardBuilder()
                    .AddButton("⬅ Назад", "")
                    .AddLine()
                    .AddButton("❌ Отключить расписание", "")
                    .Build(),
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower().StartsWith("/alarm ") && regexTime.IsMatch(e.Message.Text.Split(' ')[1]))
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            user.IsAlarmOn = true;
            user.AlarmTime = TimeOnly.Parse(e.Message.Text.Split(' ')[1]);
            user.Location = ScheduleBot.Location.Menu;
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Расписание установлено",
                Keyboard = menuKeyboard,
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "❌ отключить расписание")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            user.IsAlarmOn = false;
            user.Location = ScheduleBot.Location.Menu;
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Расписание отключено",
                Keyboard = menuKeyboard,
                PeerId = e.Message.PeerId
            });
        }
        else if (e.Message.Text.ToLower() == "⬅ назад")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            switch (user.Location)
            {
                case ScheduleBot.Location.Settings:
                    user.Location = ScheduleBot.Location.Menu;
                    await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        RandomId = Environment.TickCount,
                        Message = "Меню",
                        Keyboard = menuKeyboard,
                        PeerId = e.Message.PeerId
                    });
                    break;
                case ScheduleBot.Location.Alarm:
                    user.Location = ScheduleBot.Location.Settings;
                    await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        RandomId = Environment.TickCount,
                        Message = "Настройки",
                        Keyboard = new KeyboardBuilder()
                            .AddButton("⬅ Назад", "")
                            .AddLine()
                            .AddButton("⏲ Отправка по расписанию", "", user.IsAlarmOn ? KeyboardButtonColor.Positive : KeyboardButtonColor.Default)
                            .AddLine()
                            .AddButton("❌ Отменить регистрацию", "", KeyboardButtonColor.Negative)
                            .Build(),
                        PeerId = e.Message.PeerId
                    });
                    break;
                case ScheduleBot.Location.Menu:
                    await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                    {
                        RandomId = Environment.TickCount,
                        Message = "Ты дурак?",
                        Keyboard = menuKeyboard,
                        PeerId = e.Message.PeerId
                    });
                    break;
            }
        }
        else if (e.Message.Text.ToLower() == "❓ справка")
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "❗ВНИМАНИЕ❗\n\n" +
                "Расписание может плохо спарситься или же измениться в течение семестра.Бот не несет ответственности за такие ошибки,\n" +
                "невозможно что - то сделать без косяков(надеемся на взаимопонимание).Бот призван облегчить жизнь студентам,\n" +
                "поэтому при обнаружении ошибок – сообщать https://vk.com/top_programer или https://vk.com/sanekmethanol\n\n" +
                "⚠ Инструкция:\n\n" +
                "📌 Есть кнопочка с расписанием, а есть кнопочка с настройками\n" +
                "📌Если нажать кнопочку с настройками, откроются настройки\n" +
                "📌Если нажать кнопочку с расписанием, будет выведено расписание (ШОК!)\n" +
                "📌Если написать день недели, например Понедельник, то будет выведено расписание на этот день недели.\n" +
                "🚽Сделано WinWins и чуть-чуть Methanol на .NET 6.0.8 и C#\n" +
                "Version: " + (version.Major == 0 ? "BETA " : "") + version.ToString(),
                PeerId = e.Message.PeerId
            });
        }
        else if (Tools.DaysOfWeek.Contains(e.Message.Text.ToLower()))
        {
            User user = await GetUser(e.Message.FromId, e.Message.PeerId);
            if (user == null) return;
            int day = Tools.DaysOfWeek.ToList().IndexOf(e.Message.Text.ToLower()) + 1;
            if (day == 7)
            {
                await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
                {
                    RandomId = Environment.TickCount,
                    Message = "В воскресенье спать надо, а не на пары ходить",
                    PeerId = e.Message.PeerId
                });
                return;
            }
            Group group = groups.First(x => x.Name == user.Group);
            List<Lesson> lessons = group.Lessons.Where(x => x.DayOfWeek == (DayOfWeek)day).ToList();
            StringBuilder sb = new();
            sb.AppendLine($"Расписание на {Tools.DaysOfWeekV[day - 1]}");
            sb.AppendLine();
            foreach (var lesson in lessons)
            {
                sb.AppendLine($"⌛ Пара {lesson.Para}: {lesson.StartTime} - {lesson.EndTime}");
                sb.AppendLine($"📚 Предмет: {lesson.Name}");
                sb.AppendLine($"🏫 Аудитория: {lesson.Location}");
                sb.AppendLine("👀 " + (lesson.Type == LessonType.All ? "По числителю и знаменателю" : lesson.Type == LessonType.Numerator ? "По числителю" : "По знаменателю"));
                sb.AppendLine($"👨‍🏫 Препод: {lesson.Teacher}");
                sb.AppendLine();
            }
            await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = sb.ToString(),
                Keyboard = menuKeyboard,
                PeerId = e.Message.PeerId
            });
        }
        else
        {
            //await instance.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            //{
            //    RandomId = Environment.TickCount,
            //    Message = "Неизвестная команда",
            //    PeerId = e.Message.PeerId
            //});
        }
        string json = JsonSerializer.Serialize(users, usersJsonOptions);
        File.WriteAllText(AppRoot + "users.json", json);
    }
    catch (Exception ex)
    {
        foreach (long? user in settings.AdminUsers)
        {
            bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = "Error occurred:\n" + e.ToString(),
                UserId = user
            });
        }
    }
}

Task taskAlarm = Task.Run(async () =>
{
    while (!tokenSource.IsCancellationRequested)
    {
        List<User> alarmUsers = users.Where(x => x.IsAlarmOn && x.AlarmTime == new TimeOnly(DateTime.Now.Hour, DateTime.Now.Minute)).ToList();
        foreach (User user in alarmUsers)
        {
            Group group = groups.First(x => x.Name == user.Group);
            DateTime semStart = new DateTime(2022, 8, 29);
            DateTime nowMonday = DateTime.Now.AddDays((DateTime.Now.DayOfWeek == 0 ? -7 : -(int)DateTime.Now.DayOfWeek) + 1);
            bool isNumeric = (nowMonday - semStart).Days / 7 % 2 == 0;
            StringBuilder sb = new();
            int dw = (int)DateTime.Now.DayOfWeek;
            if (dw == 0)
            {
                dw = 1;
                sb.AppendLine("Так как сегодня воскресенье, то расписание на понедельник");
                isNumeric = !isNumeric;
            }
            else sb.AppendLine("Расписание на сегодня");
            List<Lesson> lessons = group.Lessons.Where(x => x.DayOfWeek == (DayOfWeek)dw && (x.Type == LessonType.All || x.Type == (isNumeric ? LessonType.Numerator : LessonType.Denominator))).ToList();
            lessons = lessons.OrderBy(x => x.StartTime).ToList();
            sb.AppendLine();
            foreach (var lesson in lessons)
            {
                sb.AppendLine($"⌛ Пара {lesson.Para} в {lesson.StartTime} до {lesson.EndTime}");
                sb.AppendLine($"📚 Предмет: {lesson.Name}");
                sb.AppendLine($"🏫 Аудитория: {lesson.Location}");
                sb.AppendLine($"👨‍🏫 Препод: {lesson.Teacher}");
                sb.AppendLine();
            }
            await bot.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = sb.ToString(),
                Keyboard = menuKeyboard,
                PeerId = user.Id
            });
        }
        await Task.Delay(60000, tokenSource.Token);
    }
});

async Task<User> GetUser(long? id, long? peerId)
{
    if (!users.Where(x => x.Id == id).Any())
    {
        await bot.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            RandomId = Environment.TickCount,
            Message = "Ваш ID не зарегистрирован. Чтобы зарегистрироваться напишите Начать",
            Keyboard = new KeyboardBuilder().Clear().Build(),
            PeerId = peerId
        });
        return null;
    }
    User user = users.First(x => x.Id == id);
    return user;
}

AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    Console.WriteLine("Exiting...");
    //bot.Api.Groups.DisableOnline(Convert.ToUInt64(bot.GroupId));
    foreach (long? user in settings.AdminUsers)
    {
        bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            RandomId = Environment.TickCount,
            Message = "Bot is shutting down...",
            UserId = user
        });
    }
    tokenSource.Cancel();
};

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    foreach (long? user in settings.AdminUsers)
    {
        bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            RandomId = Environment.TickCount,
            Message = "Error occurred:\n" + ((Exception)e.ExceptionObject).ToString(),
            UserId = user
        });
    }
    //Process.Start(new ProcessStartInfo() { FileName = "dotnet", Arguments = AppDomain.CurrentDomain.BaseDirectory + "ScheduleBot.dll" });
    //Environment.Exit(-1);
};

bot.OnMessageReceived += Bot_OnMessageReceived;
bot.OnException += (s, e) =>
{
    foreach (long? user in settings.AdminUsers)
    {
        bot.Api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            RandomId = Environment.TickCount,
            Message = "Error occurred:\n" + e.ToString(),
            UserId = user
        });
    }
    //if (e is not VkNet.Exception.RateLimitReachedException)
    //{
    //    Process.Start(new ProcessStartInfo() { FileName = "dotnet", Arguments = AppDomain.CurrentDomain.BaseDirectory + "ScheduleBot.dll" });
    //    Environment.Exit(-1);
    //}
};
var botTask = bot.StartAsync();

while (true)
{
    string command = Console.ReadLine();
    if (command.StartsWith("send all "))
    {
        string[] strings = command.Split(' ');
        string text = "";
        for (int i = 2; i < strings.Length; i++) text += strings[i] + " ";
        text = text.Trim();
        foreach (User user in users)
        {
            await bot.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
            {
                RandomId = Environment.TickCount,
                Message = text,
                PeerId = user.Id
            });
        }
    }
    else if (command.StartsWith("send "))
    {
        long id = long.Parse(command.Split(' ')[1]);
        string[] strings = command.Split(' ');
        string text = "";
        for (int i = 2; i < strings.Length; i++) text += strings[i] + " ";
        text = text.Trim();
        User user = users.First(users => users.Id == id);
        if (user == null) Console.WriteLine("User id does not exist");
        await bot.Api.Messages.SendAsync(new VkNet.Model.RequestParams.MessagesSendParams()
        {
            RandomId = Environment.TickCount,
            Message = text,
            PeerId = user.Id
        });
    }
}

bool IsUnix()
{
    var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    return isUnix;
}