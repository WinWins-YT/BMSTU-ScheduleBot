using System.Text.RegularExpressions;
using ScheduleBot.ExcelParser.Enums;
using ScheduleBot.ExcelParser.Models;

namespace ScheduleBot.ExcelParser.Tools
{
    internal static class Parser
    {
        internal static Lesson ParseLesson(string text, int para, DayOfWeek dayOfWeek, LessonType type = LessonType.All)
        {
            Regex regex = new(@"\s([1-7]-[0-9]{3}(\S?|(/[1-9])?)|к\.[1-9]|ул\.Моск\.-лаб\.|ОКБ ""МЭЛ"")\s");

            text = text.Replace("_", "-").Trim();
            var cabMatch = regex.Match(text + " ");
            string name = text.Substring(0, cabMatch.Index);
            string teachers = text
                .Substring(cabMatch.Index + cabMatch.Length > text.Length ? 
                    cabMatch.Index + cabMatch.Length - 1 :
                    cabMatch.Index + cabMatch.Length)
                .Trim();
            string cab = cabMatch.Value.Trim();
            if (regex.IsMatch(teachers))
            {
                var tMatch = regex.Match(teachers);
                teachers = teachers.Substring(0, tMatch.Index) + ", " + teachers.Substring(tMatch.Index + tMatch.Length - 1);
                cab += ", " + tMatch.Value.Trim();
            }
            if (teachers.Length > 30)
            {
                var temp = teachers.Split(',')[0].Split(' ').ToList();
                temp.RemoveAt(0);
                name += ", " + string.Join(' ', temp);
                teachers = teachers.Split(' ')[0] + ", " + teachers.Split(' ')[^1];
            }
            return new Lesson
            {
                Name = name,
                Teacher = teachers,
                Location = cab,
                DayOfWeek = dayOfWeek,
                Para = para,
                OriginalText = text,
                Type = type
            };
        }
    }
}