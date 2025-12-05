using System.Net;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Wraps all successful API responses into a standard format:
    /// {
    ///     "success": true,
    ///     "data": ...,
    ///     "message": ...
    /// }
    /// </summary>
    public class ResponseWrapperMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseWrapperMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Let the request continue
            var originalBodyStream = context.Response.Body;

            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;

            await _next(context);

            // If error occurred, skip wrapping (error middleware already handled it)
            if (context.Response.StatusCode >= (int)HttpStatusCode.BadRequest)
            {
                newBodyStream.Seek(0, SeekOrigin.Begin);
                await newBodyStream.CopyToAsync(originalBodyStream);
                return;
            }

            // Success: wrap the response
            newBodyStream.Seek(0, SeekOrigin.Begin);
            var originalBody = await new StreamReader(newBodyStream).ReadToEndAsync();

            object jsonBody;

            if (IsJson(originalBody))
            {
                // Wrap JSON response
                jsonBody = new
                {
                    success = true,
                    data = JsonSerializer.Deserialize<object>(originalBody),
                    message = "Request completed successfully."
                };
            }
            else
            {
                // For non-JSON responses (e.g., file download)
                jsonBody = new
                {
                    success = true,
                    data = originalBody,
                    message = "Request completed successfully."
                };
            }

            context.Response.ContentType = "application/json";
            context.Response.Body = originalBodyStream;

            await context.Response.WriteAsync(JsonSerializer.Serialize(jsonBody));
        }

        private bool IsJson(string input)
        {
            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}"))
                || (input.StartsWith("[") && input.EndsWith("]"));
        }
    }
}
