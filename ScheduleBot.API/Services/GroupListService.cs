using System.Collections;
using System.Text.Json;
using ScheduleBot.Resources.Models;

namespace ScheduleBot.API.Services;

/// <summary>
/// Сервис, который загружает JSON файл с расписанием
/// </summary>
public class GroupListService : IEnumerable<Group>
{
    private readonly List<Group> _groups = JsonSerializer.Deserialize<List<Group>>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schedule.json")));

    /// <summary>
    /// Get IEnumerator from IEnumerable
    /// </summary>
    /// <returns>IEnumerator</returns>
    public IEnumerator<Group> GetEnumerator() => _groups.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}