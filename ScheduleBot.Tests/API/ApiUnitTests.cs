using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using ScheduleBot.API.Controllers.V1;
using ScheduleBot.Resources.Models;
using Xunit;

namespace ScheduleBot.Tests.API;

public partial class ApiUnitTests
{
    [Fact]
    public void Api_OnGetOrganizations_ShouldSuccess()
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, null);
        
        // Act
        var response = controller.Organizations().ToArray();
        
        // Assert
        response.Should().NotBeNull().And.NotBeEmpty();
        response.Should().BeEquivalentTo(new[] { "Студенты" });
    }

    [Fact]
    public void Api_OnGetCourses_ShouldSuccess()
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, null);
        
        // Act
        var response = controller.SubOrganizations("Студенты").ToArray();
        
        // Assert
        response.Should().NotBeNull().And.NotBeEmpty();
        response.Should().HaveCount(8);
    }

    [Fact]
    public void Api_OnGetCourses_ShouldThrowOnWrongOrg()
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, null);
        
        // Act, assert
        Assert.Throws<ArgumentException>(() => controller.SubOrganizations("123"));
    }

    [Theory]
    [MemberData(nameof(GetCourseGroups))]
    public void Api_OnGetGroups_ShouldSuccess(IEnumerable<Group> groups, string course, string[] expectedGroups)
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, groups);
        
        // Act
        var response = controller.Groups("Студенты", course).ToArray();
        
        // Assert
        response.Should().NotBeNull().And.NotBeEmpty();
        response.Should().BeEquivalentTo(expectedGroups);
    }

    [Fact]
    public void Api_OnGetGroups_ShouldThrowOnWrongCourse()
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, null);
        
        // Act, assert
        Assert.Throws<ArgumentException>(() => controller.Groups("Студенты", "10 курс"));
    }

    [Theory]
    [MemberData(nameof(GetScheduleGroups))]
    public void Api_OnGetSchedule_ShouldSuccess(IEnumerable<Group> groups, string course, string group, int expectedCount)
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var controller = new ApiController(logger, groups);
        
        // Act
        var response = controller.GroupSchedule("Студенты", course, group).ToArray();
        
        // Assert
        response.Should().NotBeNull().And.NotBeEmpty();
        response.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void Api_OnGetSchedule_ShouldThrowOnWrongGroup()
    {
        // Arrange
        var logger = LoggerFactory.Create(x => x.AddConsole()).CreateLogger<ApiController>();
        var groups = new List<Group>
        {
            new()
            {
                Name = "Group 1",
                Course = "1"
            },
            new()
            {
                Name = "Group 2",
                Course = "2"
            }
        };
        var controller = new ApiController(logger, groups);
        
        // Act, assert
        Assert.Throws<ArgumentException>(() => controller.GroupSchedule("Студенты", "2 курс", "123"));
    }
}