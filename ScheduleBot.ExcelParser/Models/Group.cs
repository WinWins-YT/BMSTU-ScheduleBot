namespace ScheduleBot.ExcelParser.Models
{
    internal record Group
    {
        public string Name { get; set; }
        public List<Lesson> Lessons { get; } = new();
        public string Course { get; set; }
    }
}