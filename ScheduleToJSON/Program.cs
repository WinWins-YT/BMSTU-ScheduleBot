using Excel = Microsoft.Office.Interop.Excel;
using ScheduleToJSON;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Group = ScheduleToJSON.Group;
using System.Text.Json;
using Microsoft.Office.Interop.Excel;

Console.InputEncoding = System.Text.Encoding.UTF8;
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.Write("Schedule excel file: ");
string file = Console.ReadLine();
Console.WriteLine("Loading Excel module...");

Excel.Application excel = new Excel.Application();
excel.Visible = false;
Excel.Workbook workbook = excel.Workbooks.Open(file);
Excel.Worksheet sheet = null;
Console.WriteLine("Loading workbook...");

//Document options
int[][] numOfLessonPerDays = new int[][] 
{
    new int[] { 5, 4, 4, 4, 4, 3 },
    new int[] { 5, 4, 4, 5, 5, 3 },
    new int[] { 5, 5, 5, 5, 6, 3 },
    new int[] { 7, 6, 6, 7, 5, 3 },
    new int[] { 5, 4, 5, 5, 4, 3 },
    new int[] { 3, 5, 5, 3, 3, 2 },
    new int[] { 4, 4, 4, 5, 4, 3 },
    new int[] { 4, 4, 3, 3, 6, 3 }
};
int sheetCount = 8;
int[] columnCount = new int[] { 29, 28, 25, 25, 9, 6, 15, 13 };
List<Group> groups = new();

for (int sheetIndex = 1; sheetIndex <= sheetCount; sheetIndex++) {
    Console.WriteLine($"Loading sheet {sheetIndex} ({workbook.Sheets[sheetIndex].Name})...");
    sheet = workbook.Sheets[sheetIndex];
    for (int groupIndex = 3; groupIndex <= columnCount[sheetIndex - 1]; groupIndex++)
    {
        string groupName = sheet.Cells[1, groupIndex].Value2;
        Group group = new();
        group.Name = groupName;
        group.Course = sheetIndex < 7 ? sheetIndex.ToString() : sheetIndex == 7 ? "1М" : "2М";
        List<Lesson> lessons = new();
        Regex regex = new(@"\s([1-7]-[0-9]{3}(\S?|(\/[1-9])?)|к\.[1-9]|ул\.Моск\.-лаб\.|ОКБ ""МЭЛ"")\s");
        int index = 2;
        int dw = 1;
        foreach (int day in numOfLessonPerDays[sheetIndex - 1])
        {
            for (int j = index; j <= index + day * 2 - 1; j++)
            {
                string cellData = sheet.Cells[j, groupIndex].Value2;
                if (cellData == null) continue;
                cellData = cellData.Replace("_", "-").Trim();
                bool all = false, num = false;
                if (j % 2 == 0)
                {
                    all = sheet.Cells[j, groupIndex].MergeCells;
                    if (!all) num = true;
                }
                else
                {
                    all = sheet.Cells[j, groupIndex].MergeCells;
                    if (all) continue;
                    else num = false;
                }
                var cabMatch = regex.Match(cellData + " ");
                string name = cellData.Substring(0, cabMatch.Index);
                string teachers = cellData
                        .Substring(cabMatch.Index + cabMatch.Length > cellData.Length ? 
                                   cabMatch.Index + cabMatch.Length - 1 :
                                   cabMatch.Index + cabMatch.Length)
                        .Trim();
                string cab = cabMatch.Value.Trim();
                if (regex.IsMatch(teachers))
                {
                    var tMatch = regex.Match(teachers);
                    teachers = teachers.Substring(0, tMatch.Index) + ", " + teachers.Substring(tMatch.Index + tMatch.Length - 1);
                    cab += ", " + tMatch.Value.Trim();
                }
                if (teachers.Length > 30)
                {
                    var temp = teachers.Split(',')[0].Split(' ').ToList();
                    temp.RemoveAt(0);
                    name += ", " + string.Join(' ', temp);
                    teachers = teachers.Split(' ')[0] + ", " + teachers.Split(' ')[^1];
                }
                int para = Tools.RomanToArabic(sheet.Cells[j % 2 == 0 ? j : j - 1, 2].Value2.Split('\n')[0]);
                Lesson lesson = new()
                {
                    Name = name,
                    Teacher = teachers,
                    Location = cab,
                    DayOfWeek = (DayOfWeek)dw,
                    Para = para,
                    OriginalText = cellData,
                    Type = all ? LessonType.All : num ? LessonType.Numerator : LessonType.Denominator
                };
                lessons.Add(lesson);
            }
            index += day * 2;
            dw++;
        }
        group.Lessons.AddRange(lessons);
        groups.Add(group);
        Console.WriteLine("Got schedule for " + groupName);
    }
}


Console.WriteLine("Unloading Excel module...");
Marshal.ReleaseComObject(sheet);
workbook.Close();
Marshal.ReleaseComObject(workbook);
excel.Quit();
Marshal.ReleaseComObject(excel);

Console.WriteLine("Saving JSON...");
string json = JsonSerializer.Serialize(groups, new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
File.WriteAllText("schedule.json", json);
Console.WriteLine("Done");


static class Tools
{
    public static int RomanToArabic(string roman)
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

    public static int ToInt32(this string s) => Convert.ToInt32(s);

    public static TimeOnly ParaToStartTime(int para)
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
}