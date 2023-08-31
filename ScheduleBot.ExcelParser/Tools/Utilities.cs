using OfficeOpenXml;

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
    
    internal static string GetMergedRangeAddress(this ExcelRange @this)
    {
        if (@this.Merge)
        {
            var idx = @this.Worksheet.GetMergeCellId(@this.Start.Row, @this.Start.Column);
            return @this.Worksheet.MergedCells[idx-1];
        }
        else
        {
            return @this.Address;
        }
    }
}