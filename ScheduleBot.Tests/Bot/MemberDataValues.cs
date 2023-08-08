using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleBot.Resources.Enums;
using ScheduleBot.Resources.Models;

namespace ScheduleBot.Tests.Bot;

partial class VkBotUnitTests
{
    public static IEnumerable<object[]> GetLessons()
    {
        yield return new object[]
        {
            new[]
            {
                new Lesson
                {
                    Name = "Матеша",
                    Location = "Подвал",
                    Para = 1,
                    DayOfWeek = DayOfWeek.Monday,
                    Type = LessonType.All,
                    Teacher = "Кочетов"
                },
                new Lesson
                {
                    Name = "ОБЖ",
                    Location = "1-434",
                    Para = 2,
                    DayOfWeek = DayOfWeek.Monday,
                    Type = LessonType.Numerator,
                    Teacher = "Кочетов"
                }
            },
            
            DayOfWeek.Monday,
            
            "понедельник",
            
            "Расписание на понедельник\n\n" +
            "👀 По числителям и знаменателям\n" +
            "⌛ Пара 1: 08:30 - 10:05\n" +
            "📚 Предмет: Матеша\n" +
            "🏫 Аудитория: Подвал\n" +
            "👨‍🏫 Препод: Кочетов\n\n" +
            "👀 По числителям\n" +
            "⌛ Пара 2: 10:20 - 11:55\n" +
            "📚 Предмет: ОБЖ\n" +
            "🏫 Аудитория: 1-434\n" +
            "👨‍🏫 Препод: Кочетов\n\n"
        };

        yield return new object[]
        {
            new[]
            {
                new Lesson
                {
                    Name = "Калоедение",
                    Location = "3-345",
                    Para = 2,
                    DayOfWeek = DayOfWeek.Tuesday,
                    Type = LessonType.Denominator,
                    Teacher = "Кочетов"
                },
                new Lesson
                {
                    Name = "Сбор Калашникова",
                    Location = "1-448",
                    Para = 3,
                    DayOfWeek = DayOfWeek.Tuesday,
                    Type = LessonType.All,
                    Teacher = "Кочетов"
                }
            },
            
            DayOfWeek.Tuesday,
            
            "вторник",
            
            "Расписание на вторник\n\n" +
            "👀 По знаменателям\n" +
            "⌛ Пара 2: 10:20 - 11:55\n" +
            "📚 Предмет: Калоедение\n" +
            "🏫 Аудитория: 3-345\n" +
            "👨‍🏫 Препод: Кочетов\n\n" +
            "👀 По числителям и знаменателям\n" +
            "⌛ Пара 3: 12:10 - 13:45\n" +
            "📚 Предмет: Сбор Калашникова\n" +
            "🏫 Аудитория: 1-448\n" +
            "👨‍🏫 Препод: Кочетов\n\n"
        };
    }
}