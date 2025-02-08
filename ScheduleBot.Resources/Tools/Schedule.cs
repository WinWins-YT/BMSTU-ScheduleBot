namespace ScheduleBot.Resources.Tools;

/// <summary>
/// Класс утилит для расписания
/// </summary>
public static class Schedule
{
    /// <summary>
    /// Дни недели, которые может ввести пользователь бота
    /// </summary>
    public static string[] DaysOfWeek =
        { "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };

    /// <summary>
    /// Дни недели в винительном падеже
    /// </summary>
    public static string[] DaysOfWeekVerbal =
        { "воскресенье", "понедельник", "вторник", "среду", "четверг", "пятницу", "субботу" };
    
    /// <summary>
    /// Определение типа текущей недели
    /// </summary>
    /// <param name="semesterStart">Понедельник на начало семестра</param>
    /// <returns>
    /// <c>true</c> - неделя четная<br/>
    /// <c>false</c> - неделя нечетная
    /// </returns>
    public static bool IsNumeric(DateTime semesterStart)
    {
        var nowMonday = 
            DateTime.Now.AddDays((DateTime.Now.DayOfWeek == 0 ? -7 : -(int)DateTime.Now.DayOfWeek) + 1);
        var semesterStartMonday = 
            semesterStart.AddDays((semesterStart.DayOfWeek == 0 ? -7 : -(int)semesterStart.DayOfWeek) + 1);
        return (nowMonday - semesterStartMonday).Days / 7 % 2 == 0;
    }
}