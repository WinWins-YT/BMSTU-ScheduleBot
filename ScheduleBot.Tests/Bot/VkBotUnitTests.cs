using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ScheduleBot.Resources.Extensions;
using ScheduleBot.Resources.Models;
using Xunit;

namespace ScheduleBot.Tests.Bot;

public partial class VkBotUnitTests
{
    [Theory]
    [MemberData(nameof(GetLessons))]
    public void VkBot_GetSchedule_ShouldSuccess(Lesson[] lessons, DayOfWeek dayOfWeek, string showDay, string expected)
    {
        // Arrange, act
        var output = lessons.GetScheduleFor(dayOfWeek, new DateTime(2023, 8, 28), showDay);
        
        // Assert
        output.Should().NotBeNull();
        output.Should().Be(expected);
    }
}