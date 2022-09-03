using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleToJSON
{
    public record Lesson
    {
        public string Name { get; set; }
        public int Para { get; set; }
        public LessonType Type { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string Teacher { get; set; }
        public string Location { get; set; }
        public string StartTime { get => Tools.ParaToStartTime(Para).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
        public string EndTime { get => Tools.ParaToStartTime(Para).AddMinutes(95).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
    }
    
    public record Group
    {
        public string Name { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
    }

    public enum LessonType
    {
        Numerator, Denominator, All
    }

    static class Tools
    {
        public static int RomanToArabic(string roman)
        {
            return roman.ToUpper() switch
            {
                "I" => 1,
                "II" => 2,
                "III" => 3,
                "IV" => 4,
                "V" => 5,
                "VI" => 6,
                "VII" => 7,
                _ => 0
            };
        }

        public static TimeOnly ParaToStartTime(int para)
        {
            return para switch
            {
                1 => new TimeOnly(8, 30),
                2 => new TimeOnly(10, 20),
                3 => new TimeOnly(12, 10),
                4 => new TimeOnly(14, 15),
                5 => new TimeOnly(16, 5),
                6 => new TimeOnly(17, 50),
                7 => new TimeOnly(19, 35),
                _ => new TimeOnly()
            };
        }

        public static string[] DaysOfWeek => new string[] { "понедельник", "вторник", "среда", "четверг", "пятница", "суббота", "воскресенье" };
        public static string[] DaysOfWeekV => new string[] { "понедельник", "вторник", "среду", "четверг", "пятницу", "субботу", "воскресенье" };
    }
}
