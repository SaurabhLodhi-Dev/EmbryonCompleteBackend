using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application
{
    /// <summary>
    /// Handles service registrations for the Application layer.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers Application layer services into DI container.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Contact Form Services
            services.AddScoped<IContactSubmissionService, ContactSubmissionService>();

     

            // Validators
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // AutoMapper
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);


            return services;
        }
    }
}
