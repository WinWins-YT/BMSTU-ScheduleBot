using ScheduleBot.ExcelParser.Enums;
using ScheduleBot.ExcelParser.Tools;

namespace ScheduleBot.ExcelParser.Models
{
    public record Lesson
    {
        public string Name { get; set; }
        public int Para { get; set; }
        public LessonType Type { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string Teacher { get; set; }
        public string Location { get; set; }
        public string OriginalText { get; set; }
        public string StartTime => Utilities.ParaToStartTime(Para).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru"));
        public string EndTime => Utilities.ParaToStartTime(Para).AddMinutes(95).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru"));
    }
}