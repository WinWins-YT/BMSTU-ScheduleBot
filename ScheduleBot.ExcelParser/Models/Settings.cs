namespace ScheduleBot.ExcelParser.Models
{
    public record Settings
    {
        public int[][] NumberOfLessonsPerDay { get; set; }
        public int[] ColumnCount { get; set; }
        public int SheetCount { get; set; }
    }
}