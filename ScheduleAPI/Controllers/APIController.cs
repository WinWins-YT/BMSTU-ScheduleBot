using Microsoft.AspNetCore.Mvc;
using ScheduleToJSON;

namespace ScheduleAPI.Controllers
{
    [ApiController]
    [Route("/api/v1")]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        private readonly IEnumerable<Group> groups;
        public APIController(ILogger<APIController> logger, IEnumerable<Group> _groups)
        {
            _logger = logger;
            groups = _groups;
        }

        [HttpGet]
        public IEnumerable<string> Index()
        {
            return new string[] { "Студенты", "Преподаватели" };
        }

        [HttpGet]
        [Route("{org}")]
        public IEnumerable<string> Students(string org)
        {
            switch (org)
            {
                case "Студенты":
                    return new string[] { "1 курс", "2 курс", "3 курс", "4 курс", "5 курс", "6 курс", "Магистратура 1 курс", "Магистратура 2 курс" };
                default:
                    return new string[0];
            }
        }

        [HttpGet]
        [Route("{org}/{course}")]
        public IEnumerable<string> Groups(string org, string course)
        {
            switch (org)
            {
                case "Студенты":
                    return course switch
                    {
                        "1 курс" => groups.Where(x => x.Course == "1").Select(x => x.Name),
                        "2 курс" => groups.Where(x => x.Course == "2").Select(x => x.Name),
                        "3 курс" => groups.Where(x => x.Course == "3").Select(x => x.Name),
                        "4 курс" => groups.Where(x => x.Course == "4").Select(x => x.Name),
                        "5 курс" => groups.Where(x => x.Course == "5").Select(x => x.Name),
                        "6 курс" => groups.Where(x => x.Course == "6").Select(x => x.Name),
                        "Магистратура 1 курс" => groups.Where(x => x.Course == "1М").Select(x => x.Name),
                        "Магистратура 2 курс" => groups.Where(x => x.Course == "2М").Select(x => x.Name),
                        _ => new string[0]
                    };
                default:
                    return new string[0];
            }
        }

        [HttpGet]
        [Route("{org}/{course}/{group}/schedule")]
        public IEnumerable<LessonAPI> Group(string org, string course, string group)
        {
            switch (org)
            {
                case "Студенты":
                    var g = course switch
                    {
                        "1 курс" => groups.Where(x => x.Course == "1"),
                        "2 курс" => groups.Where(x => x.Course == "2"),
                        "3 курс" => groups.Where(x => x.Course == "3"),
                        "4 курс" => groups.Where(x => x.Course == "4"),
                        "5 курс" => groups.Where(x => x.Course == "5"),
                        "6 курс" => groups.Where(x => x.Course == "6"),
                        "Магистратура 1 курс" => groups.Where(x => x.Course == "1М"),
                        "Магистратура 2 курс" => groups.Where(x => x.Course == "2М"),
                        _ => new Group[0]
                    };
                    var gr = g.First(x => x.Name == group);
                    List<LessonAPI> lessons = new();
                    foreach (var item in gr.Lessons)
                    {
                        LessonAPI lesson = new();
                        lesson.Para = item.Para;
                        lesson.Name = item.Name;
                        lesson.Teacher = item.Teacher;
                        lesson.DayOfWeek = item.DayOfWeek;
                        lesson.Type = item.Type;
                        lesson.Location = item.Location;
                        lessons.Add(lesson);
                    }
                    return lessons;
                default:
                    return new LessonAPI[0];
            }
        }
    }
}
