namespace ScheduleBot.VkBot.Models;

internal record Settings
{
    public string Token { get; set; }
    public string GroupUrl { get; set; }
    public DateTime SemesterStart { get; set; }
}