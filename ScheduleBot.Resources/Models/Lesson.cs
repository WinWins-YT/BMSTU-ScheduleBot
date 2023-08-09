using ScheduleBot.Resources.Enums;
using ScheduleBot.Resources.Tools;

namespace ScheduleBot.Resources.Models
{
    /// <summary>
    /// Модель для пары
    /// </summary>
    public record Lesson
    {
        /// <summary>
        /// Название предмета
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Номер пары в расписании на день
        /// </summary>
        public int Para { get; set; }
        /// <summary>
        /// Тип пары
        /// </summary>
        public LessonType Type { get; set; }
        /// <summary>
        /// День недели пары
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }
        /// <summary>
        /// Препод предмета
        /// </summary>
        public string Teacher { get; set; }
        /// <summary>
        /// Аудитория для предмета
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Текст из таблицы без изменений
        /// </summary>
        public string OriginalText { get; set; }
        /// <summary>
        /// Время начала пары
        /// </summary>
        public string StartTime => Utilities.ParaToStartTime(Para).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru"));
        /// <summary>
        /// Время окончания пары
        /// </summary>
        public string EndTime => Utilities.ParaToStartTime(Para).AddMinutes(95).ToString("HH:mm", new System.Globalization.CultureInfo("ru-ru"));
    }
}