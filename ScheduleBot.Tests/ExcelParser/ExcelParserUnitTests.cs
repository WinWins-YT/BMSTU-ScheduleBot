using System;
using ScheduleBot.ExcelParser.Tools;
using Xunit;
using FluentAssertions;
using ScheduleBot.Resources.Enums;

namespace ScheduleBot.Tests.ExcelParser;

public class ExcelParserUnitTests
{
    [Theory]
    [InlineData("ФИЗИКА лаб. 1-414 Горбунов, Радченко",
        "ФИЗИКА лаб.",
        "Горбунов, Радченко",
        "1-414")]
    [InlineData("ЦИФРОВЫЕ ИЗМЕРИТЕЛЬНЫЕ СИСТЕМЫ лаб. I 3-309 Максимов ЯЗЫКИ ПРОГРАММИР. С УПРАВЛЯЕМЫМ КОДОМ лаб. II 3-320 Белова",
        "ЦИФРОВЫЕ ИЗМЕРИТЕЛЬНЫЕ СИСТЕМЫ лаб. I, ЯЗЫКИ ПРОГРАММИР. С УПРАВЛЯЕМЫМ КОДОМ лаб. II",
        "Максимов, Белова",
        "3-309, 3-320")]
    [InlineData("ФИЗИЧЕСКАЯ КУЛЬТУРА И СПОРТ  к.2",
        "ФИЗИЧЕСКАЯ КУЛЬТУРА И СПОРТ",
        "",
        "к.2")]
    [InlineData("ИНОСТРАННЫЙ ЯЗЫК упр. к.1 Власко, Волхонская, Колотова",
        "ИНОСТРАННЫЙ ЯЗЫК упр.",
        "Власко, Волхонская, Колотова",
        "к.1")]
    [InlineData("ИНЖЕНЕРНАЯ ГРАФИКА лаб. I 1-309 Сулина II 1-311 Вяткин",
        "ИНЖЕНЕРНАЯ ГРАФИКА лаб. I, II",
        "Сулина, Вяткин",
        "1-309, 1-311")]
    public void ExcelParser_ParseCellData_ShouldSuccess(string text,
        string expectedName, string expectedTeacher, string expectedLocation)
    {
        // Arrange
        var para = Random.Shared.Next(1, 5);
        var dayOfWeek = (DayOfWeek)Random.Shared.Next(1, 7);
        var type = (LessonType)Enum.GetValues<LessonType>().GetValue(Random.Shared.Next(0, 3))!;
        
        // Act
        var lesson = Parser.ParseLesson(text, para, dayOfWeek, type);
        
        // Assert
        lesson.Should().NotBeNull();
        lesson.Name.Should().Be(expectedName);
        lesson.Teacher.Should().Be(expectedTeacher);
        lesson.Location.Should().Be(expectedLocation);
    }
}