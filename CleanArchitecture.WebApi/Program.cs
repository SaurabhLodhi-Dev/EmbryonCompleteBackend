//using CleanArchitecture.Application;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.Application.Options;
//using CleanArchitecture.Domain.Interfaces;
//using CleanArchitecture.Infrastructure.Options;
//using CleanArchitecture.Infrastructure.Repositories;
//using CleanArchitecture.Infrastructure.Services;
//using CleanArchitecture.WebApi.BackgroundServices;
//using CleanArchitecture.WebApi.Extensions;
//using FluentValidation.AspNetCore;
//using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//using Microsoft.AspNetCore.HttpOverrides;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using Microsoft.OpenApi.Models;

//var builder = WebApplication.CreateBuilder(args);

//// =====================================================
////                LOGGING CONFIGURATION
//// =====================================================

//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

//builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
//    minimumLevel: builder.Environment.IsDevelopment()
//        ? LogLevel.Information
//        : LogLevel.Warning);

//// =====================================================
////           REQUIRED FOR REAL CLIENT IP IN PROD
//// =====================================================

//builder.Services.Configure<ForwardedHeadersOptions>(options =>
//{
//    options.ForwardedHeaders =
//        ForwardedHeaders.XForwardedFor |
//        ForwardedHeaders.XForwardedProto;

//    options.KnownNetworks.Clear();
//    options.KnownProxies.Clear();
//});

//// =====================================================
////              SERVICE REGISTRATION (DI)
//// =====================================================

//builder.Services.AddControllers();
//builder.Services.AddMemoryCache();

//// HttpClient for Geo Services
//builder.Services.AddHttpClient("GeoClient", client =>
//{
//    client.Timeout = TimeSpan.FromSeconds(5);
//    client.DefaultRequestHeaders.UserAgent.ParseAdd("EmbryonGeoClient/1.0");
//    client.DefaultRequestHeaders.Add("Accept", "application/json");
//});

//// FluentValidation
//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddFluentValidationClientsideAdapters();

//// Application + Infrastructure
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

//// =====================================================
////                       SWAGGER
//// =====================================================

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Embryon API",
//        Version = "v1",
//        Description = "Clean Architecture Production API with Real GeoLocation"
//    });
//});

//// =====================================================
////                       CAPTCHA
//// =====================================================

//builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();
//builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));

//// =====================================================
////                       EMAIL
//// =====================================================

//builder.Services.Configure<SmtpFromOptions>(builder.Configuration.GetSection("SmtpFrom"));
//builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

//builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
//builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
//builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
//builder.Services.AddHostedService<EmailSenderBackgroundService>();

//// =====================================================
////                         CORS
//// =====================================================

//var allowedOrigins = builder.Configuration
//    .GetSection("AllowedOrigins")
//    .Get<string[]>() ?? new[] { "http://localhost:5173" };

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowUI", policy =>
//    {
//        policy.WithOrigins(allowedOrigins)
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

//// =====================================================
////                 HEALTH CHECKS
//// =====================================================

//var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? throw new InvalidOperationException("Missing DefaultConnection");

//builder.Services.AddHealthChecks()
//    .AddCheck("self", () => HealthCheckResult.Healthy("API Running"))
//    .AddSqlServer(dbConnection, name: "sql_database", failureStatus: HealthStatus.Unhealthy)
//    .AddUrlGroup(new Uri("https://ipwho.is/"), name: "geo_primary", failureStatus: HealthStatus.Degraded)
//    .AddUrlGroup(new Uri("http://ip-api.com/"), name: "geo_fallback", failureStatus: HealthStatus.Degraded)
//    .AddCheck<EmailQueueHealthCheck>("email_queue", failureStatus: HealthStatus.Degraded);

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.AddServerHeader = false;
//});
//var app = builder.Build();

//// =====================================================
////                MIDDLEWARE PIPELINE
//// =====================================================

//// Must be FIRST
//app.UseForwardedHeaders();

//// =====================================================
////             SECURITY HEADERS (CSP, NoSniff, XFO)
//// =====================================================

////app.Use(async (context, next) =>
////{
////    context.Response.Headers["X-Frame-Options"] = "DENY";
////    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
////    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none';";

////    // Remove identifying headers
////    context.Response.Headers.Remove("Server");
////    context.Response.Headers.Remove("X-Powered-By");

////    await next();
////});
//app.Use(async (context, next) =>
//{
//    // SECURITY HEADERS
//    context.Response.Headers["X-Frame-Options"] = "DENY";
//    context.Response.Headers["X-Content-Type-Options"] = "nosniff";

//    context.Response.Headers["Content-Security-Policy"] =
//        "default-src 'self'; " +
//        "img-src 'self' data:; " +
//        "style-src 'self' 'unsafe-inline'; " +
//        "script-src 'self' 'unsafe-inline'; " +
//        "font-src 'self' data:; " +
//        "frame-ancestors 'none';";

//    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
//    context.Response.Headers["Pragma"] = "no-cache";
//    context.Response.Headers["Expires"] = "0";

//    context.Response.Headers.Remove("Server");
//    context.Response.Headers.Remove("X-Powered-By");

//    await next();
//});



//// =====================================================
////                   DEV ONLY MIDDLEWARE
//// =====================================================

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseRequestLogging();
//}


//// =====================================================
////                         SWAGGER
//// =====================================================

//app.UseSwagger();
//app.UseSwaggerUI();

//// HTTPS redirect
//app.UseHttpsRedirection();
//if (!app.Environment.IsDevelopment())
//{
//    app.UseHsts();
//}

//// GeoLocation middleware
//app.UseGeoLocation();

//// Global exception handler
//app.UseGlobalErrorHandler();

//// CORS
//app.UseCors("AllowUI");

//// Authentication (if enabled)
//// app.UseAuthentication();

//app.UseAuthorization();

//// Wrap API responses
//app.UseResponseWrapper();

//// Controller endpoints
//app.MapControllers();

//// Health endpoints
//app.MapHealthChecks("/live");
//app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = h => h.Tags.Contains("ready") });
//app.MapHealthChecks("/health");

//// Startup logs
//app.Logger.LogInformation("🚀 Embryon API started in {Environment}", app.Environment.EnvironmentName);
//app.Logger.LogInformation("🌍 Real GeoLocation Enabled");

//app.Run();

using CleanArchitecture.Application;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Options;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Options;
using CleanArchitecture.Infrastructure.Repositories;
using CleanArchitecture.Infrastructure.Services;
using CleanArchitecture.WebApi.BackgroundServices;
using CleanArchitecture.WebApi.Extensions;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
//     IMPORTANT: Disable Kestrel Server header early
// =====================================================
builder.WebHost.ConfigureKestrel(options =>
{
    // Prevent Kestrel from adding the Server response header
    options.AddServerHeader = false;
});

// =====================================================
//                LOGGING CONFIGURATION
// =====================================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
    minimumLevel: builder.Environment.IsDevelopment()
        ? LogLevel.Information
        : LogLevel.Warning);

// =====================================================
//           REQUIRED FOR REAL CLIENT IP IN PROD
// =====================================================

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// =====================================================
//              SERVICE REGISTRATION (DI)
// =====================================================

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// HttpClient for Geo Services
builder.Services.AddHttpClient("GeoClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EmbryonGeoClient/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// =====================================================
//                       SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Embryon API",
        Version = "v1",
        Description = "Clean Architecture Production API with Real GeoLocation"
    });

    // Optional: add security definitions here if you use JWT or others
    // c.AddSecurityDefinition(...);
});

// =====================================================
//                       CAPTCHA
// =====================================================

builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();
builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));

// =====================================================
//                       EMAIL
// =====================================================

builder.Services.Configure<SmtpFromOptions>(builder.Configuration.GetSection("SmtpFrom"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();

// =====================================================
//                         CORS
// =====================================================

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =====================================================
//                 HEALTH CHECKS
// =====================================================

var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API Running"))
    .AddSqlServer(dbConnection, name: "sql_database", failureStatus: HealthStatus.Unhealthy)
    .AddUrlGroup(new Uri("https://ipwho.is/"), name: "geo_primary", failureStatus: HealthStatus.Degraded)
    .AddUrlGroup(new Uri("http://ip-api.com/"), name: "geo_fallback", failureStatus: HealthStatus.Degraded)
    .AddCheck<EmailQueueHealthCheck>("email_queue", failureStatus: HealthStatus.Degraded);

var app = builder.Build();

// =====================================================
//                MIDDLEWARE PIPELINE (ORDER MATTERS)
// =====================================================

// Forwarded headers MUST be first
app.UseForwardedHeaders();

// =====================================================
//   HSTS (Strict-Transport-Security) - ONLY in Prod
// =====================================================
if (!app.Environment.IsDevelopment())
{
    // Adds Strict-Transport-Security header automatically
    app.UseHsts();
}

// =====================================================
// HTTPS redirect
// =====================================================
app.UseHttpsRedirection();

// =====================================================
//     SECURITY HEADERS (CSP, NoSniff, XFO, Cache-control)
// =====================================================

// We apply a slightly relaxed CSP for swagger UI in Development so the UI loads properly.
// In Production you should remove 'unsafe-inline' if possible and explicitly whitelist domains.
app.Use(async (context, next) =>
{
    // Ensure header writes happen even for static files by using OnStarting
    context.Response.OnStarting(() =>
    {
        // X-Content-Type-Options
        if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options
        if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            context.Response.Headers["X-Frame-Options"] = "DENY";

        // Cache control defaults — override per-endpoint if needed
        if (!context.Response.Headers.ContainsKey("Cache-Control"))
            context.Response.Headers["Cache-Control"] = "no-store,no-cache,must-revalidate";

        if (!context.Response.Headers.ContainsKey("Pragma"))
            context.Response.Headers["Pragma"] = "no-cache";

        if (!context.Response.Headers.ContainsKey("Expires"))
            context.Response.Headers["Expires"] = "0";

        // Remove identifying headers (Kestrel already disabled AddServerHeader, but be safe)
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        return Task.CompletedTask;
    });

    // Content-Security-Policy:
    // - In Development: allow inline scripts/styles so swagger UI works reliably
    // - In Production: prefer a stricter policy (avoid 'unsafe-inline') and whitelist specific hosts
    if (context.Request.Path.StartsWithSegments("/swagger") || app.Environment.IsDevelopment())
    {
        // Developer-friendly / Swagger-friendly CSP (development)
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data:; " +
            "style-src 'self' 'unsafe-inline'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "font-src 'self' data:; " +
            "frame-ancestors 'none';";
    }
    else
    {
        // Stricter CSP for production traffic. Adjust host list as needed.
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data:; " +
            "style-src 'self'; " +               // no 'unsafe-inline' in prod
            "script-src 'self'; " +              // no 'unsafe-inline' in prod
            "font-src 'self' data:; " +
            "frame-ancestors 'none';";
    }

    await next();
});

// =====================================================
//                   DEV ONLY MIDDLEWARE
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseRequestLogging();
}

// =====================================================
//                         SWAGGER
// =====================================================

// NOTE: Swagger UI ships its own JS/CSS which ZAP sometimes flags as "vulnerable JS library".
// To reduce false positives in production: upgrade Swashbuckle to the latest stable version
// and serve static swagger assets from your app (or CDN with SRI). This is a maintenance task — not a runtime fix.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Embryon API v1");
    c.RoutePrefix = "swagger";
    // Optional small ergonomics:
    c.DefaultModelsExpandDepth(-1); // collapse schemas
});

// =====================================================
// GeoLocation middleware (must run before controllers if it sets context)
// =====================================================
app.UseGeoLocation();

// Global exception handler
app.UseGlobalErrorHandler();

// CORS
app.UseCors("AllowUI");

// Authentication (if enabled)
// app.UseAuthentication();

app.UseAuthorization();

// Wrap API responses (should be before controllers so it intercepts JSON)
app.UseResponseWrapper();

// Controller mapping
app.MapControllers();

// Health checks
app.MapHealthChecks("/live");
app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = h => h.Tags.Contains("ready") });
app.MapHealthChecks("/health");

// Startup logs
app.Logger.LogInformation("🚀 Embryon API started in {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("🌍 Real GeoLocation Enabled");

app.Run();

