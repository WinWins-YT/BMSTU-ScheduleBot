using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ScheduleBot.API.Models;

namespace ScheduleBot.API.ActionFilters;

/// <summary>
/// Action filter для исключений
/// </summary>
public class ExceptionFilterAttribute : Attribute, IExceptionFilter
{
    /// <summary>
    /// Обработка исключения во время выполнения запроса
    /// </summary>
    /// <param name="context">Контекст запроса</param>
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ArgumentException exception:
                HandleBadRequest(context, exception);
                break;
            
            default:
                HandleInternalError(context);
                break;
        }
    }

    private void HandleBadRequest(ExceptionContext context, Exception exception)
    {
        var jsonResult = new JsonResult(
            new ErrorResponse(
                HttpStatusCode.BadRequest, 
                exception.Message))
        {
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        context.Result = jsonResult;
    }

    private void HandleInternalError(ExceptionContext context)
    {
        var jsonResult = new JsonResult(
            new ErrorResponse(
                HttpStatusCode.InternalServerError,
                "Что-то поломалось, оно само. Сообщение: " + context.Exception.Message))
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };

        context.Result = jsonResult;
    }
}