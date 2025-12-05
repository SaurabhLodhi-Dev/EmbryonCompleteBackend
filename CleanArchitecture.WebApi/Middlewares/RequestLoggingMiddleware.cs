using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Logs all incoming HTTP requests + outgoing responses.
    /// Captures method, route, duration, request body, query params, IP, etc.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Capture request details
            var request = context.Request;

            string method = request.Method;
            string path = request.Path;
            string query = request.QueryString.HasValue ? request.QueryString.Value! : "";
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string userAgent = request.Headers["User-Agent"].ToString();

            // Read body (only for POST/PUT/PATCH with small payload)
            string bodyContent = "";
            if (request.ContentLength < 102_400 &&  // 100 KB limit
                (method == "POST" || method == "PUT" || method == "PATCH"))
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                bodyContent = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            // Run the pipeline
            await _next(context);

            stopwatch.Stop();

            int statusCode = context.Response.StatusCode;

            var logObject = new
            {
                method,
                path,
                query,
                requestBody = TryParseJson(bodyContent),
                statusCode,
                ip,
                userAgent,
                durationMs = stopwatch.ElapsedMilliseconds
            };

            _logger.LogInformation("HTTP Request Log: {log}", JsonSerializer.Serialize(logObject));
        }

        private object? TryParseJson(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            try
            {
                return JsonSerializer.Deserialize<object>(raw);
            }
            catch
            {
                return raw;
            }
        }
    }
}
