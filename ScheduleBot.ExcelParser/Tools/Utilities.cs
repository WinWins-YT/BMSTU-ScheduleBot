namespace ScheduleBot.ExcelParser.Tools
{
    internal static class Utilities
    {
        internal static TimeOnly ParaToStartTime(int para)
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
        
        internal static int RomanToArabic(string roman)
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
    }
}