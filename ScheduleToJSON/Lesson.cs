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
        public string OriginalText { get; set; }
        public string StartTime { get => Tools.ParaToStartTime(Para).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
        public string EndTime { get => Tools.ParaToStartTime(Para).AddMinutes(95).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
    }
    
    public record Group
    {
        public string Name { get; set; }
        public List<Lesson> Lessons { get; } = new();
        public string Course { get; set; }
    }

    public enum LessonType
    {
        Numerator, Denominator, All
    }
}
