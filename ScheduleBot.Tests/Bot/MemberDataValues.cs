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

            "Расписание на понедельник" + Environment.NewLine + Environment.NewLine +
            "👀 По числителям и знаменателям" + Environment.NewLine +
            "⌛ Пара 1: 08:30 - 10:05" + Environment.NewLine +
            "📚 Предмет: Матеша" + Environment.NewLine +
            "🏫 Аудитория: Подвал" + Environment.NewLine +
            "👨‍🏫 Препод: Кочетов" + Environment.NewLine + Environment.NewLine +
            "👀 По числителям" + Environment.NewLine +
            "⌛ Пара 2: 10:20 - 11:55" + Environment.NewLine +
            "📚 Предмет: ОБЖ" + Environment.NewLine +
            "🏫 Аудитория: 1-434" + Environment.NewLine +
            "👨‍🏫 Препод: Кочетов" + Environment.NewLine + Environment.NewLine
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
            
            "Расписание на вторник" + Environment.NewLine + Environment.NewLine +
            "👀 По знаменателям" + Environment.NewLine +
            "⌛ Пара 2: 10:20 - 11:55" + Environment.NewLine +
            "📚 Предмет: Калоедение" + Environment.NewLine +
            "🏫 Аудитория: 3-345" + Environment.NewLine +
            "👨‍🏫 Препод: Кочетов" + Environment.NewLine + Environment.NewLine +
            "👀 По числителям и знаменателям" + Environment.NewLine +
            "⌛ Пара 3: 12:10 - 13:45" + Environment.NewLine +
            "📚 Предмет: Сбор Калашникова" + Environment.NewLine +
            "🏫 Аудитория: 1-448" + Environment.NewLine +
            "👨‍🏫 Препод: Кочетов" + Environment.NewLine + Environment.NewLine
        };
    }
}