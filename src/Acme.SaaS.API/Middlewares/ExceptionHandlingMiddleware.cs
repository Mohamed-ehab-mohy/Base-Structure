using System.Net;
using System.Text.Json;
using Acme.SaaS.Application.Common.DTOs;
using Acme.SaaS.Application.Exceptions;
using Acme.SaaS.Domain.Exceptions;

namespace Acme.SaaS.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ApiResponse<object>.Fail(exception.Message)),
            DomainException => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail(exception.Message)),
            ValidationException ve => (HttpStatusCode.BadRequest, ApiResponse<object>.Fail(exception.Message, ve.Errors)),
            ForbiddenException => (HttpStatusCode.Forbidden, ApiResponse<object>.Fail(exception.Message)),
            _ => (HttpStatusCode.InternalServerError, ApiResponse<object>.Fail("An internal error occurred."))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
