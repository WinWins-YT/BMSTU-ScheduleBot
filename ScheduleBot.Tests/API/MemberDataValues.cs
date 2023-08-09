using System.Collections.Generic;
using ScheduleBot.Resources.Models;

namespace ScheduleBot.Tests.API;

public partial class ApiUnitTests
{
    public static IEnumerable<object[]> GetCourseGroups()
    {
        yield return new object[]
        {
            new[]
            {
                new Group
                {
                    Name = "Group 1",
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 2",
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 3",
                    Course = "2"
                }
            },
            "1 курс",
            new[] { "Group 1", "Group 2" }
        };

        yield return new object[]
        {
            new[]
            {
                new Group
                {
                    Name = "Group 1",
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 2",
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 3",
                    Course = "2"
                }
            },
            "2 курс",
            new[] { "Group 3" }
        };
    }

    public static IEnumerable<object[]> GetScheduleGroups()
    {
        yield return new object[]
        {
            new[]
            {
                new Group
                {
                    Name = "Group 1",
                    Course = "1",
                    Lessons = new List<Lesson>
                    {
                        new(), new(), new()
                    }
                },
                new Group
                {
                    Name = "Group 2",
                    Lessons = new List<Lesson>
                    {
                        new(), new()
                    },
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 3",
                    Course = "2",
                    Lessons = new List<Lesson>
                    {
                        new(), new(), new(), new()
                    }
                }
            },
            "1 курс",
            "Group 1",
            3
        };
        
        yield return new object[]
        {
            new[]
            {
                new Group
                {
                    Name = "Group 1",
                    Course = "1",
                    Lessons = new List<Lesson>
                    {
                        new(), new(), new()
                    }
                },
                new Group
                {
                    Name = "Group 2",
                    Lessons = new List<Lesson>
                    {
                        new(), new()
                    },
                    Course = "1"
                },
                new Group
                {
                    Name = "Group 3",
                    Course = "2",
                    Lessons = new List<Lesson>
                    {
                        new(), new(), new(), new()
                    }
                }
            },
            "2 курс",
            "Group 3",
            4
        };
    }
}