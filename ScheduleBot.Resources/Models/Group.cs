namespace ScheduleBot.Resources.Models
{
    /// <summary>
    /// Модель для группы
    /// </summary>
    public record Group
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Список пар у группы
        /// </summary>
        public List<Lesson> Lessons { get; set; } = new();
        /// <summary>
        /// Курс группы
        /// </summary>
        public string Course { get; set; }
    }
}