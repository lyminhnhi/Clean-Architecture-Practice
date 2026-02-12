using System.Net;
using System.Text.Json;

namespace CodeLeap.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request: {Path}", context.Request.Path);

            context.Response.ContentType = "application/json";

            var (statusCode, message) = MapException(ex);

            context.Response.StatusCode = statusCode;

            var response = new
            {
                statusCode,
                message
            };

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }

        private (int statusCode, string message) MapException(Exception ex)
        {
            return ex switch
            {
                ArgumentException => ((int)HttpStatusCode.BadRequest, ex.Message),

                KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),

                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access"),

                _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
            };
        }
    }
}