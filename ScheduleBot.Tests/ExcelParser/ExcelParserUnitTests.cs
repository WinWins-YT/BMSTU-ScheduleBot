using System;
using System.Collections.Generic;
using ScheduleBot.ExcelParser.Enums;
using ScheduleBot.ExcelParser.Models;
using ScheduleBot.ExcelParser.Tools;
using Xunit;
using FluentAssertions;

namespace ScheduleBot.Tests.ExcelParser;

public class ExcelParserUnitTests
{
    [Theory]
    [InlineData("ФИЗИКА лаб. 1-414 Горбунов, Радченко", 
        2, 
        (DayOfWeek)3, 
        LessonType.All,
        "ФИЗИКА лаб.",
        "Горбунов, Радченко",
        "1-414")]
    [InlineData("ЦИФРОВЫЕ ИЗМЕРИТЕЛЬНЫЕ СИСТЕМЫ лаб. I 3-309 Максимов ЯЗЫКИ ПРОГРАММИР. С УПРАВЛЯЕМЫМ КОДОМ лаб. II 3-320 Белова",
        4, 
        (DayOfWeek)5, 
        LessonType.Numerator,
        "ЦИФРОВЫЕ ИЗМЕРИТЕЛЬНЫЕ СИСТЕМЫ лаб. I, ЯЗЫКИ ПРОГРАММИР. С УПРАВЛЯЕМЫМ КОДОМ лаб. II",
        "Максимов, Белова",
        "3-309, 3-320")]
    [InlineData("ФИЗИЧЕСКАЯ КУЛЬТУРА И СПОРТ  к.2", 
        3, 
        (DayOfWeek)5, 
        LessonType.All,
        "ФИЗИЧЕСКАЯ КУЛЬТУРА И СПОРТ",
        "",
        "к.2")]
    [InlineData("ИНОСТРАННЫЙ ЯЗЫК упр. к.1 Власко, Волхонская, Колотова", 
        2, 
        (DayOfWeek)1, 
        LessonType.All,
        "ИНОСТРАННЫЙ ЯЗЫК упр.",
        "Власко, Волхонская, Колотова",
        "к.1")]
    [InlineData("ИНЖЕНЕРНАЯ ГРАФИКА лаб. I 1-309 Сулина II 1-311 Вяткин", 
        4, 
        (DayOfWeek)2, 
        LessonType.Denominator,
        "ИНЖЕНЕРНАЯ ГРАФИКА лаб. I, II",
        "Сулина, Вяткин",
        "1-309, 1-311")]
    public void ExcelParser_ParseCellData_ShouldSuccess(string text, int para, DayOfWeek dayOfWeek, LessonType type, 
        string expectedName, string expectedTeacher, string expectedLocation)
    {
        // Arrange, Act
        var lesson = Parser.ParseLesson(text, para, dayOfWeek, type);
        
        // Assert
        lesson.Should().NotBeNull();
        lesson.Name.Should().Be(expectedName);
        lesson.Teacher.Should().Be(expectedTeacher);
        lesson.Location.Should().Be(expectedLocation);
    }
}