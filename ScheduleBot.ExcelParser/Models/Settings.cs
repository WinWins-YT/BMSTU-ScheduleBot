namespace ScheduleBot.ExcelParser.Models
{
    internal record Settings
    {
        public int[][] NumberOfLessonsPerDay { get; set; }
        public int[] ColumnCount { get; set; }
        public int SheetCount { get; set; }
    }
}