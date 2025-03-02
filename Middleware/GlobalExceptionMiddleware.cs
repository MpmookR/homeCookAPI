using System.Net;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            // Continue the request pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "An unhandled exception occurred.");

            // Handle the exception and return a proper response
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        int statusCode = exception switch
        {
            ArgumentNullException => (int)HttpStatusCode.BadRequest,  // 400 Bad Request
            KeyNotFoundException => (int)HttpStatusCode.NotFound,  // 404 Not Found
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,  // 401 Unauthorized
            _ => (int)HttpStatusCode.InternalServerError // 500 Internal Server Error (Default)
        };

        var errorId = Guid.NewGuid().ToString(); // Unique error tracking ID

        var errorResponse = new
        {
            message = "An error occurred while processing your request.",
            errorId,  // Include error ID for debugging purposes
            statusCode,
            exceptionMessage = exception.Message // ⚠️ Remove this in production for security
        };

        response.StatusCode = statusCode;
        return response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}
