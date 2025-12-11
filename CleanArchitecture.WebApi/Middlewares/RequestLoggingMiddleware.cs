//using CleanArchitecture.Domain.Entities;
//using CleanArchitecture.Infrastructure.Data;
//using Humanizer;
//using System.Diagnostics;
//using System.Text;
//using System.Text.Json;
//using CleanArchitecture.WebApi.Extensions;

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
//        private readonly IServiceProvider _serviceProvider; // ★ Added for DB logging

//        public RequestLoggingMiddleware(
//            RequestDelegate next,
//            ILogger<RequestLoggingMiddleware> logger,
//            IHostEnvironment env,
//            IServiceProvider serviceProvider) // ★ Added
//        {
//            _next = next;
//            _logger = logger;
//            _env = env;
//            _serviceProvider = serviceProvider; // ★ Added
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
//                durationMs = _env.IsDevelopment() ? stopwatch.ElapsedMilliseconds : 0,
//                requestBody = shouldLogBody ? TryParseJson(bodyContent ?? "") : null
//            };

//            _logger.LogInformation("HTTP Request Log: {log}", JsonSerializer.Serialize(logObject));

//            // -------------------------------------------------------
//            // ★ NEW: Save PageVisit to database
//            // -------------------------------------------------------
//            //    try
//            //    {
//            //        using var scope = _serviceProvider.CreateScope();
//            //        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//            //        var visit = new PageVisit
//            //        {
//            //            PageUrl = context.Request.Path + context.Request.QueryString,
//            //            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
//            //            UserAgent = context.Request.Headers["User-Agent"],
//            //            Referrer = context.Request.Headers["Referer"]
//            //        };

//            //        db.PageVisits.Add(visit);
//            //        await db.SaveChangesAsync();
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        _logger.LogError(ex, "Failed to save PageVisit");
//            //    }
//            //}

//            // -------------------------------------------------------
//            // Replace DB writes with structured Serilog/ILogger event
//            // -------------------------------------------------------
//            try
//            {
//                // Mask or reduce size of long user agents / paths to avoid excessive log size
//                var truncatedPath = context.Request.Path + context.Request.QueryString;
//                if (truncatedPath.Length > 1000) truncatedPath = truncatedPath.Substring(0, 1000);

//                _logger.LogInformation("PageVisit recorded: {Path} {StatusCode} {IP} {UA} {Referrer}",
//                    truncatedPath,
//                    context.Response?.StatusCode,
//                    context.Connection.RemoteIpAddress?.ToString(),
//                    context.Request.Headers["User-Agent"].ToString().Truncate(300),
//                    context.Request.Headers["Referer"].ToString().Truncate(300));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to write PageVisit log");
//            }
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
using Humanizer;
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
        private readonly IServiceProvider _serviceProvider;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _serviceProvider = serviceProvider;
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
                request.ContentLength < 102_400 &&
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

            // Continue with pipeline
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

                // ✔ Mask sensitive fields
                requestBody = shouldLogBody ? MaskSensitive(bodyContent ?? "") : null
            };

            _logger.LogInformation(
                "HTTP Request Log: {log}",
                JsonSerializer.Serialize(logObject)
            );

            // -------------------------------------------------------
            // Replace DB writes with structured Serilog/ILogger event
            // -------------------------------------------------------
            try
            {
                var truncatedPath = context.Request.Path + context.Request.QueryString;
                if (truncatedPath.Length > 1000)
                    truncatedPath = truncatedPath.Substring(0, 1000);

                _logger.LogInformation(
                    "PageVisit recorded: {Path} {StatusCode} {IP} {UA} {Referrer}",
                    truncatedPath,
                    context.Response?.StatusCode,
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Headers["User-Agent"].ToString().Truncate(300),
                    context.Request.Headers["Referer"].ToString().Truncate(300)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write PageVisit log");
            }
        }

        // -------------------------------------------------------
        // ⭐ NEW: Mask sensitive user-submitted fields in logs
        // -------------------------------------------------------
        private string MaskSensitive(string rawJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawJson))
                    return rawJson;

                var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(rawJson);
                if (dict == null)
                    return rawJson;

                string[] sensitive =
                {
                    "email", "password", "phone", "message",
                    "firstName", "lastName", "subject"
                };

                foreach (var key in sensitive)
                {
                    var match = dict.Keys.FirstOrDefault(k =>
                        string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

                    if (match != null)
                        dict[match] = "***masked***";
                }

                return JsonSerializer.Serialize(dict);
            }
            catch
            {
                return "***unparsable body***";
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
