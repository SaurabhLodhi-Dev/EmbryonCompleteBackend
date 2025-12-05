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

        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Continue execution
                await _next(context);
            }
            catch (Exception ex)
            {
                // Handle all uncaught exceptions
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            HttpStatusCode statusCode;
            object errorResponse;

            switch (ex)
            {
                // Validation Errors (FluentValidation)
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
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

                // Not Found
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = new
                    {
                        success = false,
                        message = ex.Message
                    };
                    break;

                // SQL or DB Failure
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new
                    {
                        success = false,
                        message = ex.Message
                    };
                    break;

                // All other exceptions
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    errorResponse = new
                    {
                        success = false,
                        message = "An unexpected error occurred.",
                        detail = _env.IsDevelopment() ? ex.ToString() : null  // ONLY in dev
                    };
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}
