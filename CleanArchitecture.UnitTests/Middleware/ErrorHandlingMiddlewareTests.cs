using CleanArchitecture.WebApi.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitecture.UnitTests.Middleware
{
    public class ErrorHandlingMiddlewareTests
    {
        private DefaultHttpContext CreateContext()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();
            return ctx;
        }

        private string ReadResponseBody(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
            string body = reader.ReadToEnd();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

        private IServiceProvider MockServiceProvider()
        {
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(Mock.Of<IServiceProvider>());

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IServiceScopeFactory)))
              .Returns(scopeFactory.Object);

            return sp.Object;
        }

        [Fact]
        public async Task Should_Handle_ValidationException()
        {
            var next = new RequestDelegate(_ => throw new ValidationException("Invalid Email"));

            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Development");

            var middleware = new ErrorHandlingMiddleware(next, env.Object, MockServiceProvider());
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);

            var json = ReadResponseBody(context);
            Assert.Contains("Validation failed", json);
        }

        [Fact]
        public async Task Should_Handle_NotFound()
        {
            var next = new RequestDelegate(_ => throw new KeyNotFoundException("Not found!"));

            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");

            var middleware = new ErrorHandlingMiddleware(next, env.Object, MockServiceProvider());
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task Should_Handle_GenericException()
        {
            var next = new RequestDelegate(_ => throw new Exception("Boom!"));

            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns("Production");

            var middleware = new ErrorHandlingMiddleware(next, env.Object, MockServiceProvider());
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

            var json = ReadResponseBody(context);
            Assert.Contains("An unexpected error occurred", json);
        }
    }
}
