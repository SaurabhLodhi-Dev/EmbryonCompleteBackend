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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
    // Accept forwarded headers from any reverse proxy (Cloudflare, Nginx, IIS, AWS ALB etc.)
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Clear restrictions → allow ANY proxy IP
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// =====================================================
//              SERVICE REGISTRATION (DI)
// =====================================================

builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// ----------------- HttpClient: GeoService ------------
builder.Services.AddHttpClient("GeoClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EmbryonGeoClient/1.0");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ----------------- FluentValidation ------------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// ----------------- Application + Infrastructure -------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ----------------- Swagger ---------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Embryon API",
        Version = "v1",
        Description = "Clean Architecture Production API with Real GeoLocation"
    });
});

// ----------------- CAPTCHA ----------------------------
builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();
builder.Services.Configure<CaptchaOptions>(builder.Configuration.GetSection("Captcha"));

// ----------------- EMAIL -----------------------------
builder.Services.Configure<SmtpFromOptions>(builder.Configuration.GetSection("SmtpFrom"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();

// ----------------- CORS -------------------------------
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
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// =====================================================
//                 HEALTH CHECKS
// =====================================================

var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API Running"))

    .AddSqlServer(
        dbConnection,
        name: "sql_database",
        failureStatus: HealthStatus.Unhealthy)

    .AddUrlGroup(
        new Uri("https://ipwho.is/"),
        name: "geo_primary",
        failureStatus: HealthStatus.Degraded)

    .AddUrlGroup(
        new Uri("http://ip-api.com/"),
        name: "geo_fallback",
        failureStatus: HealthStatus.Degraded)

    .AddCheck<EmailQueueHealthCheck>(
        "email_queue",
        failureStatus: HealthStatus.Degraded);

var app = builder.Build();

// =====================================================
//                MIDDLEWARE PIPELINE (ORDER IS CRITICAL)
// =====================================================

// 1️⃣ MUST COME FIRST → Enables real client IP forwarding
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseRequestLogging();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// HTTPS Redirection
app.UseHttpsRedirection();

// 2️⃣ GeoLocation Middleware (now receives REAL remote IP)
app.UseGeoLocation();

// Global Error Handler
app.UseGlobalErrorHandler();

// CORS
app.UseCors("AllowUI");

// Authorization
app.UseAuthorization();

// Response Wrapper
app.UseResponseWrapper();

// Controller Mapping
app.MapControllers();

// =====================================================
//                HEALTH ENDPOINTS
// =====================================================

app.MapHealthChecks("/live");
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = h => h.Tags.Contains("ready")
});
app.MapHealthChecks("/health");

// =====================================================
//                STARTUP LOGGING
// =====================================================

app.Logger.LogInformation("🚀 Embryon API started in {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("🌍 Real GeoLocation Enabled");

app.Run();
