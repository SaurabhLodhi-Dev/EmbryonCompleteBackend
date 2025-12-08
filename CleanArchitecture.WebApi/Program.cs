using CleanArchitecture.Application;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Options;
using CleanArchitecture.Infrastructure.Repositories;
using CleanArchitecture.Infrastructure.Services;
using CleanArchitecture.WebApi.BackgroundServices;
using CleanArchitecture.WebApi.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;

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
//              SERVICE REGISTRATION (DI)
// =====================================================

// ----------------- Controllers -----------------------
builder.Services.AddControllers();

// ----------------- HttpClient: GeoService ------------
builder.Services.AddHttpClient("GeoClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EmbryonGeoClient/1.0");
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
        Description = "Clean Architecture Production API"
    });
});

// ----------------- CAPTCHA ----------------------------
builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();
builder.Services.Configure<CaptchaOptions>(
    builder.Configuration.GetSection("Captcha"));

// ----------------- EMAIL -----------------------------
builder.Services.Configure<CleanArchitecture.Application.Options.SmtpFromOptions>(
    builder.Configuration.GetSection("SmtpFrom"));
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();

// ----------------- CORS -------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
                           "https://your-production-ui.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =====================================================
//                 HEALTH CHECKS
// =====================================================

// Safely load connection string
var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection in appsettings.json");

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())

    // SQL Server check
    .AddSqlServer(
        dbConnection,
        name: "sql_database",
        tags: new[] { "ready" })

    // External API (Geo API)
    .AddUrlGroup(
        new Uri("https://ipwho.is/"),
        name: "geo_api",
        tags: new[] { "ready" })

    // Email Queue health
    .AddCheck<EmailQueueHealthCheck>("email_queue", tags: new[] { "ready" });

var app = builder.Build();

// =====================================================
//                MIDDLEWARE PIPELINE
// =====================================================

// ------------ Development Logging --------------------
if (app.Environment.IsDevelopment())
{
    app.UseRequestLogging();
}

// ------------ Swagger UI -----------------------------
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Embryon API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// ------------ GeoLocation Middleware ------------------
app.UseGeoLocation();

// ------------ Global Error Handler --------------------
app.UseGlobalErrorHandler();

app.UseCors("AllowUI");

app.UseAuthorization();

// ------------ Response Wrapper -------------------------
app.UseResponseWrapper();

// ------------ Controllers ------------------------------
app.MapControllers();

// =====================================================
//                HEALTH ENDPOINTS
// =====================================================

// Liveness – app running?
app.MapHealthChecks("/live");

// Readiness – dependencies ready? DB/API/QUEUE
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = h => h.Tags.Contains("ready"),
});

// Full JSON health report
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
});

app.Run();
