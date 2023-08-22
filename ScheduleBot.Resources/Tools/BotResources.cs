using System.Runtime.InteropServices;

namespace ScheduleBot.Resources.Tools;

/// <summary>
/// Класс с ресурсами для бота
/// </summary>
public static class BotResources
{
    /// <summary>
    /// Строка с текстом для справки бота
    /// </summary>
    public static string HelpString => "❗ВНИМАНИЕ❗\n\n" +
                                       "Расписание может плохо спарситься или же измениться в течение семестра. Бот не несет ответственности за такие ошибки,\n" +
                                       "невозможно что - то сделать без косяков(надеемся на взаимопонимание).Бот призван облегчить жизнь студентам,\n" +
                                       "поэтому при обнаружении ошибок – сообщать https://vk.com/top_programer или https://vk.com/sanekmethanol\n\n" +
                                       "⚠ Инструкция:\n\n" +
                                       "📌 Есть кнопочка с днями недели, а есть кнопочка с настройками\n" +
                                       "📌 Если нажать кнопочку с настройками, откроются настройки\n" +
                                       "📌 Если нажать на день недели, например Понедельник, то будет выведено расписание на этот день недели.\n" +
                                       "🚽 Сделано WinWins и чуть-чуть Methanol на .NET";

    /// <summary>
    /// Строка с информацией о системе
    /// </summary>
    public static string ServerInfoString
    {
        get
        {
            var system = new SystemLoad();

            return $"OS version: {Environment.OSVersion}" + Environment.NewLine +
                   $"Running {RuntimeInformation.FrameworkDescription}" + Environment.NewLine +
                   $"Number of logical processors: {Environment.ProcessorCount}" + Environment.NewLine +
                   $"CPU Model: {system.CpuModel}" + Environment.NewLine +
                   $"CPU Usage: {Math.Round(system.CpuLoad)}%" + Environment.NewLine +
                   $"Memory usage: {Math.Round(system.UsedMemory)} MB/{Math.Round(system.TotalMemory)} MB, " +
                   $"{Math.Round(system.UsedMemory / system.TotalMemory * 100)}%, {Math.Round(system.FreeMemory)} MB free" + Environment.NewLine +
                   $"System uptime: {system.UpTime}";
        }
    }  
}