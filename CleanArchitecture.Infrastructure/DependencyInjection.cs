using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // -----------------------------
        // Database Context
        // -----------------------------
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // -----------------------------
        // Repositories
        // -----------------------------
        services.AddScoped<IContactSubmissionRepository, ContactSubmissionRepository>();
 
        services.AddScoped<ICountryRepository, CountryRepository>();

        // -----------------------------
        // Email Settings
        // (Reads "EmailSettings" from appsettings.json)
        // -----------------------------
      

        // -----------------------------
        // Email Service (MailKit + SMTP)
        // -----------------------------


        return services;
    }
}
