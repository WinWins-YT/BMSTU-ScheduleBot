using ScheduleBot.Resources.Enums;

namespace ScheduleBot.Resources.Models;

/// <summary>
/// Модель пользователя бота
/// </summary>
public record User
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public long? Id { get; set; }
    /// <summary>
    /// Группа пользователя
    /// </summary>
    public string Group { get; set; }
    /// <summary>
    /// Флаг, включено ли раписание у пользователя
    /// </summary>
    public bool IsAlarmOn { get; set; }
    /// <summary>
    /// Время на которое установлено расписание
    /// </summary>
    public TimeOnly AlarmTime { get; set; }
    /// <summary>
    /// В каком месте меню находится пользователь
    /// </summary>
    public Location Location { get; set; }
}