//using CleanArchitecture.Application;
//using CleanArchitecture.Application.Email;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.Domain.Interfaces;
//using CleanArchitecture.Infrastructure;
//using CleanArchitecture.Infrastructure.Options;
//using CleanArchitecture.Infrastructure.Repositories;
//using CleanArchitecture.Infrastructure.Services;
//using CleanArchitecture.WebApi.BackgroundServices;
//using CleanArchitecture.WebApi.Extensions;
//using FluentValidation.AspNetCore;
//using Microsoft.OpenApi.Models;

//var builder = WebApplication.CreateBuilder(args);

//// Clear default providers (optional but cleaner)
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();




//// Add file logger with environment-specific settings
//if (builder.Environment.IsDevelopment())
//{
//    // Verbose logs in Dev
//    builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
//        minimumLevel: LogLevel.Information);
//}
//else
//{
//    // Production: only warnings & errors
//    builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
//        minimumLevel: LogLevel.Warning);
//}
//// -----------------------------------------------------
////                 SERVICE REGISTRATION
//// -----------------------------------------------------

//builder.Services.AddControllers();
//builder.Services.AddHttpClient();

//// FluentValidation
//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddFluentValidationClientsideAdapters();

//// Application + Infrastructure
//builder.Services.AddApplication();
//builder.Services.AddInfrastructure(builder.Configuration);

//// Swagger (always enabled for IIS + Production)
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Embryon API",
//        Version = "v1",
//        Description = "Clean Architecture Production API"
//    });
//});

//builder.Services.AddTransient<ICaptchaValidator, CaptchaValidator>();

//// -------------------------
////       CAPTCHA SYSTEM
//// -------------------------
//builder.Services.Configure<CaptchaOptions>(
//    builder.Configuration.GetSection("Captcha"));


//// -------------------------
////       EMAIL SYSTEM
//// -------------------------

//builder.Services.Configure<CleanArchitecture.Application.Options.SmtpFromOptions>(
//    builder.Configuration.GetSection("SmtpFrom"));
//builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

//builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
//builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
//builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
//builder.Services.AddHostedService<EmailSenderBackgroundService>();

//// -------------------------
////        CORS
//// -------------------------

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowUI", policy =>
//    {
//        policy
//            .WithOrigins("http://localhost:5173")   // React App
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

////builder.Logging.AddFile("Logs/embryon-log-{Date}.txt");

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseRequestLogging(); // only in dev
//}

//// -----------------------------------------------------
////                 MIDDLEWARE PIPELINE
//// -----------------------------------------------------

//// Enable Swagger ALWAYS (IIS Production needs this)
//app.UseSwagger();
//app.UseSwaggerUI(c =>
//{
//    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Embryon API v1");
//    c.RoutePrefix = "swagger";
//});

//app.UseHttpsRedirection();

//// Detect Client Country + IP before controllers
//app.UseGeoLocation();

//// Log all API requests
//app.UseRequestLogging();

//// Global exception handler
//app.UseGlobalErrorHandler();

//app.UseCors("AllowUI");

//app.UseAuthorization();

//// Wrap all API success responses
//app.UseResponseWrapper();

//app.MapControllers();

//app.Run();


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
//                 LOGGING CONFIGURATION
// -----------------------------------------------------

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// File logger settings based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
        minimumLevel: LogLevel.Information);
}
else
{
    builder.Logging.AddFile("Logs/embryon-log-{Date}.txt",
        minimumLevel: LogLevel.Warning);
}

// -----------------------------------------------------
//                 SERVICE REGISTRATION
// -----------------------------------------------------

builder.Services.AddControllers();
// -----------------------------------------------------
//      HTTP CLIENTS (GeoClient with timeout + user-agent)
// -----------------------------------------------------
builder.Services.AddHttpClient("GeoClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(3);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("EmbryonGeoClient/1.0");
});


// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Application + Infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Swagger
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

// CAPTCHA
builder.Services.Configure<CaptchaOptions>(
    builder.Configuration.GetSection("Captcha"));

// EMAIL
builder.Services.Configure<CleanArchitecture.Application.Options.SmtpFromOptions>(
    builder.Configuration.GetSection("SmtpFrom"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddHostedService<EmailSenderBackgroundService>();


//// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowUI", policy =>
//    {
//        policy.WithOrigins("http://localhost:5173")
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://your-production-ui.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// -----------------------------------------------------
//                 MIDDLEWARE PIPELINE
// -----------------------------------------------------

// DEV MODE: verbose request logging
if (app.Environment.IsDevelopment())
{
    app.UseRequestLogging();
}

// Swagger always enabled
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Embryon API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// Detect Client Country + IP before controllers
app.UseGeoLocation();

// ❌ REMOVE THIS (was double logging)
// app.UseRequestLogging();

app.UseGlobalErrorHandler();

app.UseCors("AllowUI");

app.UseAuthorization();

// Wrap all API success responses
app.UseResponseWrapper();

app.MapControllers();

app.Run();
