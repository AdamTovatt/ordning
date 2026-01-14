using Microsoft.AspNetCore.Http;
using Ordning.Server.Database;
using System.Net;
using System.Text.Json;

namespace Ordning.Server.Middleware
{
    /// <summary>
    /// Middleware that handles exceptions globally and converts them to appropriate HTTP responses.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
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

        /// <summary>
        /// Handles exceptions and converts them to appropriate HTTP responses.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="exception">The exception to handle.</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            string message;

            switch (exception)
            {
                case DatabaseConstraintViolationException dbEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = dbEx.Message;
                    _logger.LogWarning(exception, "Database constraint violation: {Message}", dbEx.Message);
                    break;

                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = argEx.Message;
                    _logger.LogWarning(exception, "Invalid argument: {Message}", argEx.Message);
                    break;

                case InvalidOperationException opEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = opEx.Message;
                    _logger.LogWarning(exception, "Invalid operation: {Message}", opEx.Message);
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "An error occurred while processing your request.";
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            string json = JsonSerializer.Serialize(new { message }, options);
            await context.Response.WriteAsync(json);
        }
    }
}
