
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Global exception handling middleware.
    /// Ensures every exception returns a clean, consistent error response in JSON.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider; // ★ Added

        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, IServiceProvider serviceProvider) // ★ Added
        {
            _next = next;
            _env = env;
            _serviceProvider = serviceProvider; // ★ Added
        }

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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode;
            object errorResponse;

            switch (ex)
            {
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    errorResponse = new
                    {
                        success = false,
                        message = "Validation failed",
                        errors = validationException.Errors.Select(e => new
                        {
                            field = e.PropertyName,
                            error = e.ErrorMessage
                        })
                    };
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    errorResponse = new
                    {
                        success = false,
                        message = ex.Message
                    };
                    break;

                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    errorResponse = new
                    {
                        success = false,
                        message = ex.Message
                    };
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError; // 500
                    errorResponse = new
                    {
                        success = false,
                        message = "An unexpected error occurred.",
                        detail = _env.IsDevelopment() ? ex.ToString() : null
                    };
                    break;
            }

            // -------------------------------------------------------
            // ★ NEW: Save only CRITICAL (500) errors to database
            // -------------------------------------------------------
            if ((int)statusCode >= 500) // Only log server errors
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var log = new ErrorLog
                    {
                        ErrorMessage = ex.Message,
                        StackTrace = ex.ToString(),
                        Endpoint = context.Request.Path,
                        HttpMethod = context.Request.Method,
                        UserAgent = context.Request.Headers["User-Agent"],
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };

                    db.ErrorLogs.Add(log);
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // Never throw inside global error handler
                }
            }

            // -------------------------------------------------------
            // Return error JSON response (your existing logic)
            // -------------------------------------------------------
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
