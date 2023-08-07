namespace ScheduleBot.Resources.Tools;

public static class Schedule
{
    public static string[] DaysOfWeek =
        { "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };

    public static string[] DaysOfWeekVerbal =
        { "воскресенье", "понедельник", "вторник", "среду", "четверг", "пятницу", "субботу" };
    
    public static bool IsNumeric(DateTime semesterStart)
    {
        var nowMonday = 
            DateTime.Now.AddDays((DateTime.Now.DayOfWeek == 0 ? -7 : -(int)DateTime.Now.DayOfWeek) + 1);
        return (nowMonday - semesterStart).Days / 7 % 2 == 0;
    }
}