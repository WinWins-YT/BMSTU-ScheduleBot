namespace ScheduleBot.Resources.Models
{
    public record Group
    {
        public string Name { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
        public string Course { get; set; }
    }
}