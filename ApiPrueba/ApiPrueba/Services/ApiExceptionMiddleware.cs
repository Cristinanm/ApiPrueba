using System.Net;

namespace ApiPrueba.Services;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (EntityNotFoundException exception)
        {
            await WriteError(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (BusinessRuleException exception)
        {
            await WriteError(context, HttpStatusCode.Conflict, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error no controlado");
            await WriteError(context, HttpStatusCode.InternalServerError, "Ocurrió un error interno.");
        }
    }

    private static Task WriteError(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.StatusCode = (int)status;
        return context.Response.WriteAsJsonAsync(new
        {
            status = (int)status,
            error = message
        });
    }
}
