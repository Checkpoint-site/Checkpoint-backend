using Domain.Exceptions.Common;

namespace Api.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger
)
{

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu uma exceção não tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            ValidationException => StatusCodes.Status400BadRequest,
            ValueErrorException => StatusCodes.Status400BadRequest,
            TypeErrorException => StatusCodes.Status400BadRequest,
            DomainException => StatusCodes.Status422UnprocessableEntity,
            IntegrationException => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        object result = exception switch
        {
            ValidationException valEx when valEx.Errors.Count > 0 => new
            {
                error = exception.Message,
                statusCode = context.Response.StatusCode,
                errors = valEx.Errors
            },
            _ => new
            {
                error = exception.Message,
                statusCode = context.Response.StatusCode
            }
        };

        return context.Response.WriteAsJsonAsync(result);
    }
}
