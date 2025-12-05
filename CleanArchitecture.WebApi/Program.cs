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
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------
//                 SERVICE REGISTRATION
// -----------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger (always enabled for IIS + Production)
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

builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();

// -------------------------
//       CAPTCHA SYSTEM
// -------------------------
builder.Services.Configure<CaptchaOptions>(
    builder.Configuration.GetSection("Captcha"));


// -------------------------
//       EMAIL SYSTEM
// -------------------------

builder.Services.Configure<CleanArchitecture.Application.Options.SmtpFromOptions>(
    builder.Configuration.GetSection("SmtpFrom"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();

// -------------------------
//        CORS
// -------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")   // React App
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Logging.AddFile("Logs/embryon-log-{Date}.txt");

var app = builder.Build();

// -----------------------------------------------------
//                 MIDDLEWARE PIPELINE
// -----------------------------------------------------

// Enable Swagger ALWAYS (IIS Production needs this)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Embryon API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Detect Client Country + IP before controllers
app.UseGeoLocation();

// Log all API requests
app.UseRequestLogging();

// Global exception handler
app.UseGlobalErrorHandler();

app.UseCors("AllowUI");

app.UseAuthorization();

// Wrap all API success responses
app.UseResponseWrapper();

app.MapControllers();

app.Run();
