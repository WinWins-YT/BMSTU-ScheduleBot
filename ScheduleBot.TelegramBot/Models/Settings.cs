namespace ScheduleBot.TelegramBot.Models;

internal record Settings
{
    public string Token { get; set; } = "";
    public DateTime SemesterStart { get; set; }
}