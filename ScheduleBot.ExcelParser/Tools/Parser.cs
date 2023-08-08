using System.Text.RegularExpressions;
using ScheduleBot.Resources.Enums;
using ScheduleBot.Resources.Models;

namespace ScheduleBot.ExcelParser.Tools
{
    internal static class Parser
    {
        internal static Lesson ParseLesson(string text, int para, DayOfWeek dayOfWeek, LessonType type = LessonType.All)
        {
            Regex regex = new(@"\s([1-7]-[0-9]{3}(\S?|(/[1-9])?)|к\.[1-7]|ул\.Моск\.-лаб\." +
                              @"|ОКБ ""МЭЛ""|НПП ""Тайфун""|ООО ""РИТЦ"")\s");

            text = text.Replace("_", "-").Trim();
            if (regex.Matches(text + " ").Count == 0)
                throw new ArgumentException("Wrong cell data was passed", nameof(text));
            
            var cabMatch = regex.Match(text + " ");
            string name = text.Substring(0, cabMatch.Index).Trim();
            string teachers = text
                .Substring(cabMatch.Index + cabMatch.Length > text.Length ? 
                    cabMatch.Index + cabMatch.Length - 1 :
                    cabMatch.Index + cabMatch.Length)
                .Trim();
            string cab = cabMatch.Value.Trim();
            if (regex.IsMatch(teachers))
            {
                var tMatch = regex.Match(teachers);
                teachers = teachers.Substring(0, tMatch.Index).Trim() + ", " + 
                           teachers.Substring(tMatch.Index + tMatch.Length - 1).Trim();
                cab += ", " + tMatch.Value.Trim();
            }
            if (teachers.Length > 30)
            {
                var temp = teachers.Split(',')[0].Split(' ').ToList();
                temp.RemoveAt(0);
                name += ", " + string.Join(' ', temp);
                teachers = teachers.Split(' ')[0].Trim() + ", " + teachers.Split(' ')[^1].Trim();
            }
            var reg = new Regex(@"\sI{1,3}\b");
            if (reg.IsMatch(teachers))
            {
                var mMatch = reg.Match(teachers);
                name += $", {mMatch.Value.Trim()}";
                teachers = teachers.Remove(mMatch.Index, mMatch.Length).Trim();
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