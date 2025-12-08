using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitecture.UnitTests.Middleware
{
    public class ResponseWrapperMiddlewareTests
    {
        private DefaultHttpContext CreateContext()
        {
            var ctx = new DefaultHttpContext();
            ctx.Response.Body = new MemoryStream();
            return ctx;
        }

        private string ReadBody(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
            string text = reader.ReadToEnd();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }

        [Fact]
        public async Task Should_Wrap_Success_Response()
        {
            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync("{\"hello\":\"world\"}");
            });

            var middleware = new ResponseWrapperMiddleware(next);
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            var json = ReadBody(context);

            Assert.Contains("\"success\":true", json);
            Assert.Contains("\"hello\":\"world\"", json);
        }

        [Fact]
        public async Task Should_Not_Wrap_Error_Response()
        {
            var next = new RequestDelegate(async ctx =>
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsync("{\"error\":\"bad\"}");
            });

            var middleware = new ResponseWrapperMiddleware(next);
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            var json = ReadBody(context);

            Assert.Contains("\"error\":\"bad\"", json);
            Assert.DoesNotContain("\"success\":true", json);
        }

    }
}
