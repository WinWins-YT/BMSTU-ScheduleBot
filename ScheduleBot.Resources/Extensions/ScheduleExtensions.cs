using System.Text;
using ScheduleBot.Resources.Enums;
using ScheduleBot.Resources.Models;
using ScheduleBot.Resources.Tools;

namespace ScheduleBot.Resources.Extensions;

public static class ScheduleExtensions
{
    public static string GetScheduleFor(this IEnumerable<Lesson> lessons, DayOfWeek dayOfWeek, DateTime semesterStart, string showDayOfWeek = "сегодня")
    {
        StringBuilder output = new();
        var isNumeric = Schedule.IsNumeric(semesterStart);
        
        var dw = (int)dayOfWeek;
        if (dw == 0)
        {
            dw = 1;
            isNumeric = !isNumeric;
            output.AppendLine("Так как " + showDayOfWeek + " воскресенье, то расписание на понедельник (" +
                          (isNumeric ? "числитель" : "знаменатель") + ")");
        }
        else
        {
            if (showDayOfWeek != "сегодня" && showDayOfWeek != "завтра")
                output.AppendLine("Расписание на " + showDayOfWeek);
            else
                output.AppendLine($"Расписание на {showDayOfWeek} ({(isNumeric ? "числитель" : "знаменатель")})");
        }

        List<Lesson> lessonsList = new();
        
        if (showDayOfWeek != "сегодня" && showDayOfWeek != "завтра")
            lessonsList = lessons.Where(x => x.DayOfWeek == (DayOfWeek)dw)
                .OrderBy(x => x.StartTime).ToList();
        else
            lessonsList = lessons.Where(x =>
                x.DayOfWeek == (DayOfWeek)dw && (x.Type == LessonType.All ||
                                                 x.Type == (isNumeric
                                                     ? LessonType.Numerator
                                                     : LessonType.Denominator)))
                .OrderBy(x => x.StartTime).ToList();

        output.AppendLine();
        
        foreach (var lesson in lessonsList)
        {
            if (showDayOfWeek != "сегодня" && showDayOfWeek != "завтра")
            {
                output.AppendLine("👀 По " + (lesson.Type == LessonType.All ? "числителям и знаменателям" :
                    lesson.Type == LessonType.Numerator ? "числителям" : "знаменателям"));
            }
            output.AppendLine($"⌛ Пара {lesson.Para}: {lesson.StartTime} - {lesson.EndTime}");
            output.AppendLine($"📚 Предмет: {lesson.Name}");
            output.AppendLine($"🏫 Аудитория: {lesson.Location}");
            output.AppendLine(lesson.Teacher != "" ? $"👨‍🏫 Препод: {lesson.Teacher}" : "👨‍🏫 Препод не указан");
            output.AppendLine();
        }

        return output.ToString();
    }
}