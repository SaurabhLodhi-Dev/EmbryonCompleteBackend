using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace CleanArchitecture.UnitTests.WebApi
{
    public class TestServerFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace SQL Server
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                // InMemory DB
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Fake services
                services.AddSingleton<ICaptchaValidator, FakeCaptchaValidator>();
                services.AddSingleton<IEmailQueue, FakeEmailQueue>();

                // Replace HttpClient factory globally
                services.AddSingleton<IHttpClientFactory, FakeHttpClientFactory>();
            });
        }
    }
}
