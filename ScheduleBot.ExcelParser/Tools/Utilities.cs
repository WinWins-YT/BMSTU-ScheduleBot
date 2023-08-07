namespace ScheduleBot.ExcelParser.Tools;

internal static class Utilities
{
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