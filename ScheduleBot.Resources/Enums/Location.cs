namespace ScheduleBot.Resources.Enums;

/// <summary>
/// Обозначает в каком месте меню находится пользователь бота
/// </summary>
public enum Location
{
    /// <summary>
    /// Пользователь на этапе ввода группы при регистрации
    /// </summary>
    Registration, 
    /// <summary>
    /// Пользователь в меню
    /// </summary>
    Menu, 
    /// <summary>
    /// Пользователь в настройках
    /// </summary>
    Settings, 
    /// <summary>
    /// Пользователь в настройках расписания
    /// </summary>
    Alarm, 
    /// <summary>
    /// Пользователь на этапе ввода времени расписания
    /// </summary>
    AlarmSet
}