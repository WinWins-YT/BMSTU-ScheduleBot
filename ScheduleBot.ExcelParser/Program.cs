using System.Text.Json;
using OfficeOpenXml;
using ScheduleBot.ExcelParser.Models;
using ScheduleBot.ExcelParser.Tools;
using ScheduleBot.Resources.Enums;
using Group = ScheduleBot.Resources.Models.Group;

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

if (!File.Exists("settings.json"))
{
    Console.WriteLine("Couldn't find settings.json file. See README file here: https://github.com/WinWins-YT/BMSTU-ScheduleBot");
    return;
}

Console.WriteLine("Using settings.json file");
var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("settings.json"));

Console.Write("Enter Excel schedule file path: ");
var file = Console.ReadLine()!;

Console.WriteLine("Loading Excel file...");
var excel = new ExcelPackage(new FileInfo(file));

settings.SheetCount = excel.Workbook.Worksheets.Count;

List<Group> groups = new();

for (int sheetIndex = 0; sheetIndex < settings.SheetCount; sheetIndex++)
{
    Console.WriteLine($"Loading sheet {sheetIndex + 1} ({excel.Workbook.Worksheets[sheetIndex].Name})...");
    var sheet = excel.Workbook.Worksheets[sheetIndex];
    for (int groupIndex = 3; groupIndex <= settings.ColumnCount[sheetIndex]; groupIndex++)
    {
        var groupName = sheet.Cells[1, groupIndex].Text;
        var group = new Group
        {
            Name = groupName,
            Course = sheetIndex < 6 ? (sheetIndex + 1).ToString() : sheetIndex == 6 ? "1М" : "2М"
        };
        
        int index = 2, dw = 1;

        foreach (var day in settings.NumberOfLessonsPerDay[sheetIndex])
        {
            for (var j = index; j <= index + day * 2 - 1; j++)
            {
                var cellBaseAddress = sheet.Cells[j, groupIndex].GetMergedRangeAddress();
                var cellData = sheet.Cells[cellBaseAddress].Text;
                if (string.IsNullOrEmpty(cellData)) 
                    continue;
                
                bool all = false, num = false;
                if (j % 2 == 0)
                {
                    all = sheet.Cells[cellBaseAddress].Rows > 1;
                    if (!all) 
                        num = true;
                }
                else
                {
                    if (sheet.Cells[cellBaseAddress].Rows > 1)
                        continue;
                }

                var type = all ? LessonType.All : num ? LessonType.Numerator : LessonType.Denominator;

                var lesson = Parser.ParseLesson(
                    cellData,
                    Utilities.RomanToArabic(sheet.Cells[j % 2 == 0 ? j : j - 1, 2].Text.Split('\n')[0]),
                    (DayOfWeek)dw,
                    type
                );
                group.Lessons.Add(lesson);
            }
            
            index += day * 2;
            dw++;
        }
        
        groups.Add(group);
        Console.WriteLine($"Got schedule for {groupName}");
    }
}

Console.WriteLine("Unloading Excel file...");
excel.Dispose();

Console.WriteLine("Saving JSON...");
string json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
File.WriteAllText("schedule.json", json);
Console.WriteLine("Done");