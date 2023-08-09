using Microsoft.AspNetCore.Mvc;
using ScheduleBot.Resources.Models;

namespace ScheduleBot.API.Controllers.V1;

/// <summary>
/// API контроллер с расписанием
/// </summary>
[ApiController]
[Route("api/v1")]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;
    private readonly IEnumerable<Group> _groups;

    /// <summary>
    /// API контроллер с расписанием
    /// </summary>
    /// <param name="logger">Логгер</param>
    /// <param name="groups"></param>
    public ApiController(ILogger<ApiController> logger, IEnumerable<Group> groups)
    {
        _logger = logger;
        _groups = groups;
    }

    /// <summary>
    /// Получение списка организаций
    /// </summary>
    /// <returns>Массив строк с организациями</returns>
    [HttpGet]
    public IEnumerable<string> Organizations()
    {
        _logger.LogInformation("Got organizations");
        return new[] { "Студенты" };
    }

    /// <summary>
    /// Получение списка курсов для организации
    /// </summary>
    /// <param name="org">Организация</param>
    /// <returns>Массив строк с курсами</returns>
    /// <exception cref="ArgumentException">Если такой организации не существует</exception>
    [HttpGet("{org}")]
    public IEnumerable<string> SubOrganizations(string org)
    {
        _logger.LogInformation("Got suborganizations of {Org}", org);
        return org switch
        {
            "Студенты" => new[]
            {
                "1 курс", "2 курс", "3 курс", "4 курс", "5 курс", "6 курс", "Магистратура 1 курс", "Магистратура 2 курс"
            },
            _ => throw new ArgumentException("Такой организации не существует", nameof(org))
        };
    }

    /// <summary>
    /// Получение списка групп для курса
    /// </summary>
    /// <param name="org">Организация</param>
    /// <param name="course">Курс</param>
    /// <returns>Массив строк с группами</returns>
    /// <exception cref="ArgumentException">Если такого курса или организации не существует</exception>
    [HttpGet("{org}/{course}")]
    public IEnumerable<string> Groups(string org, string course)
    {
        _logger.LogInformation("Got group of {Course} of {Org}", course, org);
        return org switch
        {
            "Студенты" => course switch
            {
                "1 курс" => _groups.Where(x => x.Course == "1").Select(x => x.Name),
                "2 курс" => _groups.Where(x => x.Course == "2").Select(x => x.Name),
                "3 курс" => _groups.Where(x => x.Course == "3").Select(x => x.Name),
                "4 курс" => _groups.Where(x => x.Course == "4").Select(x => x.Name),
                "5 курс" => _groups.Where(x => x.Course == "5").Select(x => x.Name),
                "6 курс" => _groups.Where(x => x.Course == "6").Select(x => x.Name),
                "Магистратура 1 курс" => _groups.Where(x => x.Course == "1М").Select(x => x.Name),
                "Магистратура 2 курс" => _groups.Where(x => x.Course == "2М").Select(x => x.Name),
                _ => throw new ArgumentException("Такого курса не существует", nameof(course))
            },
            _ => throw new ArgumentException("Такой организации не существует", nameof(org))
        };
    }

    /// <summary>
    /// Получение списка объектов пары
    /// </summary>
    /// <param name="org">Организация</param>
    /// <param name="course">Курс</param>
    /// <param name="group">Группа</param>
    /// <returns>Массив строк с объектами пары</returns>
    /// <exception cref="ArgumentException">Если такой группы, курса или организации не существует</exception>
    [HttpGet("{org}/{course}/{group}/schedule")]
    public IEnumerable<Lesson> GroupSchedule(string org, string course, string group)
    {
        _logger.LogInformation("Got schedule of {Group} of {Course} of {Org}", group, course, org);
        var grs = Groups(org, course);
        if (!grs.Contains(group))
            throw new ArgumentException("Этой группы в этом курсе не существует");

        return _groups.First(x => x.Name == group).Lessons;
    }
}