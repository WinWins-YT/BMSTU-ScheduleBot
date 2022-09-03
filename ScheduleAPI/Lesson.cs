using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public record LessonAPI
    {
        [JsonPropertyName("title")]
        public string Name { get; set; }
        [JsonPropertyName("number")]
        public int Para { get; set; }
        [JsonIgnore]
        public LessonType Type { get; set; }
        [JsonPropertyName("week_type")]
        public string WeekType { get => Type == LessonType.All ? "all" : Type == LessonType.Numerator ? "odd" : "even"; }
        [JsonConverter(typeof(JsonDayOfWeekConverter)), JsonPropertyName("day")]
        public DayOfWeek DayOfWeek { get; set; }
        [JsonPropertyName("lecturer")]
        public string Teacher { get; set; }
        [JsonPropertyName("classroom")]
        public string Location { get; set; }
        [JsonPropertyName("time_start")]
        public string StartTime { get => Tools.ParaToStartTime(Para).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
        [JsonPropertyName("time_end")]
        public string EndTime { get => Tools.ParaToStartTime(Para).AddMinutes(95).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru")); }
    }

    public record Group
    {
        public string Name { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
        public string Course { get; set; }
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

        public static int ToInt32(this string s) => Convert.ToInt32(s);

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

    class JsonDayOfWeekConverter : JsonConverter<DayOfWeek>
    {
        public override DayOfWeek Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Enum.Parse<DayOfWeek>(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DayOfWeek value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Enum.GetName(value));
        }
    }
}
