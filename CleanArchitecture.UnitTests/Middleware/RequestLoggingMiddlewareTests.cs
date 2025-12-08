using System.IO;
using System.Threading.Tasks;
using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace CleanArchitecture.UnitTests.Middleware
{
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async Task Should_Log_Request()
        {
            // Mock logger
            var logger = new Mock<ILogger<RequestLoggingMiddleware>>();

            // Delegate executed after middleware
            var next = new RequestDelegate(async ctx =>
            {
                await ctx.Response.WriteAsync("OK");
            });

            // New constructor dependencies
            var env = Mock.Of<IHostEnvironment>(e => e.EnvironmentName == "Development");
            var serviceProvider = Mock.Of<IServiceProvider>();

            // Create middleware
            var middleware = new RequestLoggingMiddleware(next, logger.Object, env, serviceProvider);

            // Fake HTTP context
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Invoke
            await middleware.InvokeAsync(context);

            // Verify logging
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }
}
