

using System.Net;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Wraps successful API responses in a standard format:
    /// {
    ///    success = true,
    ///    data = ...,
    ///    message = "Request completed successfully."
    /// }
    /// Skips wrapping on health endpoints and non-JSON responses.
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
            var path = context.Request.Path.Value?.ToLower();

            // -------------------------------------------------------
            // 1. EXCLUDE HEALTH CHECK ENDPOINTS
            // -------------------------------------------------------
            if (path is "/health" or "/ready" or "/live")
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;

            await using var tempStream = new MemoryStream();
            context.Response.Body = tempStream;

            await _next(context);

            // -------------------------------------------------------
            // 2. IF ERROR → do NOT wrap
            // -------------------------------------------------------
            if (context.Response.StatusCode >= (int)HttpStatusCode.BadRequest)
            {
                tempStream.Seek(0, SeekOrigin.Begin);
                await tempStream.CopyToAsync(originalBody);
                return;
            }

            // -------------------------------------------------------
            // 3. READ ORIGINAL RESPONSE BODY
            // -------------------------------------------------------
            tempStream.Seek(0, SeekOrigin.Begin);
            var raw = await new StreamReader(tempStream).ReadToEndAsync();

            // Prevent null/empty body wrapping issues
            object? parsedData = null;

            if (IsJson(raw))
            {
                parsedData = JsonSerializer.Deserialize<object>(raw);
            }
            else if (!string.IsNullOrWhiteSpace(raw))
            {
                parsedData = raw;
            }

            var wrappedResponse = new
            {
                success = true,
                data = parsedData,
                message = "Request completed successfully."
            };

            // -------------------------------------------------------
            // 4. WRITE FINAL WRAPPED JSON
            // -------------------------------------------------------
            context.Response.ContentType = "application/json";
            context.Response.Body = originalBody;

            var finalJson = JsonSerializer.Serialize(wrappedResponse);
            await context.Response.WriteAsync(finalJson);
        }

        private static bool IsJson(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            input = input.Trim();
            return (input.StartsWith("{") && input.EndsWith("}")) ||
                   (input.StartsWith("[") && input.EndsWith("]"));
        }
    }
}
