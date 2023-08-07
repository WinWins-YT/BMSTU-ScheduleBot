using ScheduleBot.Resources.Enums;

namespace ScheduleBot.Resources.Models;

public record User
{
    public long? Id { get; set; }
    public string Group { get; set; }
    public bool IsAlarmOn { get; set; }
    public TimeOnly AlarmTime { get; set; }
    public Location Location { get; set; }
}