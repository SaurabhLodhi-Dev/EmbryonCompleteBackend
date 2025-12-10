//using System.Diagnostics;
//using System.Text;
//using System.Text.Json;

//namespace CleanArchitecture.WebApi.Middlewares
//{
//    /// <summary>
//    /// Logs all incoming HTTP requests + outgoing responses.
//    /// Captures method, route, duration, request body, query params, IP, etc.
//    /// </summary>
//    public class RequestLoggingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<RequestLoggingMiddleware> _logger;

//        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
//        {
//            _next = next;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            // Capture request details
//            var request = context.Request;

//            string method = request.Method;
//            string path = request.Path;
//            string query = request.QueryString.HasValue ? request.QueryString.Value! : "";
//            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
//            string userAgent = request.Headers["User-Agent"].ToString();

//            // Read body (only for POST/PUT/PATCH with small payload)
//            string bodyContent = "";
//            if (request.ContentLength < 102_400 &&  // 100 KB limit
//                (method == "POST" || method == "PUT" || method == "PATCH"))
//            {
//                request.EnableBuffering();
//                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
//                bodyContent = await reader.ReadToEndAsync();
//                request.Body.Position = 0;
//            }

//            // Run the pipeline
//            await _next(context);

//            stopwatch.Stop();

//            int statusCode = context.Response.StatusCode;

//            var logObject = new
//            {
//                method,
//                path,
//                query,
//                requestBody = TryParseJson(bodyContent),
//                statusCode,
//                ip,
//                userAgent,
//                durationMs = stopwatch.ElapsedMilliseconds
//            };

//            _logger.LogInformation("HTTP Request Log: {log}", JsonSerializer.Serialize(logObject));
//        }

//        private object? TryParseJson(string raw)
//        {
//            if (string.IsNullOrWhiteSpace(raw)) return null;

//            try
//            {
//                return JsonSerializer.Deserialize<object>(raw);
//            }
//            catch
//            {
//                return raw;
//            }
//        }
//    }
//}



//using System.Diagnostics;
//using System.Text;
//using System.Text.Json;

//namespace CleanArchitecture.WebApi.Middlewares
//{
//    /// <summary>
//    /// Logs incoming HTTP requests + outgoing responses.
//    /// In Production: minimal logs
//    /// In Development or Debug: includes small request bodies
//    /// </summary>
//    public class RequestLoggingMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly ILogger<RequestLoggingMiddleware> _logger;
//        private readonly IHostEnvironment _env;

//        public RequestLoggingMiddleware(
//            RequestDelegate next,
//            ILogger<RequestLoggingMiddleware> logger,
//            IHostEnvironment env)
//        {
//            _next = next;
//            _logger = logger;
//            _env = env;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            var stopwatch = Stopwatch.StartNew();

//            var request = context.Request;

//            string method = request.Method;
//            string path = request.Path;
//            string query = request.QueryString.HasValue ? request.QueryString.Value! : "";
//            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
//            string userAgent = request.Headers["User-Agent"].ToString();

//            // Only log request body in Development or Debug-level logging
//            bool shouldLogBody =
//                _env.IsDevelopment() &&
//                request.ContentLength < 102_400 && // 100 KB limit
//                (method == "POST" || method == "PUT" || method == "PATCH") &&
//                _logger.IsEnabled(LogLevel.Debug);

//            string? bodyContent = null;

//            if (shouldLogBody)
//            {
//                request.EnableBuffering();
//                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
//                bodyContent = await reader.ReadToEndAsync();
//                request.Body.Position = 0;
//            }

//            // Execute next middleware in pipeline
//            await _next(context);

//            stopwatch.Stop();

//            int statusCode = context.Response.StatusCode;

//            var logObject = new
//            {
//                method,
//                path,
//                query,
//                statusCode,
//                ip,
//                userAgent,
//                durationMs = stopwatch.ElapsedMilliseconds,
//                requestBody = shouldLogBody ? TryParseJson(bodyContent ?? "") : null
//            };

//            _logger.LogInformation("HTTP Request Log: {log}", JsonSerializer.Serialize(logObject));
//        }

//        private object? TryParseJson(string raw)
//        {
//            if (string.IsNullOrWhiteSpace(raw)) return null;

//            try
//            {
//                return JsonSerializer.Deserialize<object>(raw);
//            }
//            catch
//            {
//                return raw;
//            }
//        }
//    }
//}


using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Logs incoming HTTP requests + outgoing responses.
    /// In Production: minimal logs
    /// In Development or Debug: includes small request bodies
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly IServiceProvider _serviceProvider; // ★ Added for DB logging

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IHostEnvironment env,
            IServiceProvider serviceProvider) // ★ Added
        {
            _next = next;
            _logger = logger;
            _env = env;
            _serviceProvider = serviceProvider; // ★ Added
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var request = context.Request;

            string method = request.Method;
            string path = request.Path;
            string query = request.QueryString.HasValue ? request.QueryString.Value! : "";
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            string userAgent = request.Headers["User-Agent"].ToString();

            // Only log request body in Development or Debug-level logging
            bool shouldLogBody =
                _env.IsDevelopment() &&
                request.ContentLength < 102_400 && // 100 KB limit
                (method == "POST" || method == "PUT" || method == "PATCH") &&
                _logger.IsEnabled(LogLevel.Debug);

            string? bodyContent = null;

            if (shouldLogBody)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                bodyContent = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            // Execute next middleware in pipeline
            await _next(context);

            stopwatch.Stop();

            int statusCode = context.Response.StatusCode;

            var logObject = new
            {
                method,
                path,
                query,
                statusCode,
                ip,
                userAgent,
                durationMs = _env.IsDevelopment() ? stopwatch.ElapsedMilliseconds : 0,
                requestBody = shouldLogBody ? TryParseJson(bodyContent ?? "") : null
            };

            _logger.LogInformation("HTTP Request Log: {log}", JsonSerializer.Serialize(logObject));

            // -------------------------------------------------------
            // ★ NEW: Save PageVisit to database
            // -------------------------------------------------------
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var visit = new PageVisit
                {
                    PageUrl = context.Request.Path + context.Request.QueryString,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"],
                    Referrer = context.Request.Headers["Referer"]
                };

                db.PageVisits.Add(visit);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save PageVisit");
            }
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
