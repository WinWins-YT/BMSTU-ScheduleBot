using System.Net;

namespace ScheduleBot.API.Models;

/// <summary>
/// Модель для ошибок во время выполнения запроса
/// </summary>
/// <param name="StatusCode">Код статуса</param>
/// <param name="Message">Сообщение</param>
public record ErrorResponse(HttpStatusCode StatusCode, string Message);