using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace CleanArchitecture.UnitTests.Middleware
{
    public class GeoLocationMiddlewareTests
    {
        private HttpClient CreateHttpClient(string responseJson)
        {
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                });

            return new HttpClient(handler.Object);
        }

        [Fact]
        public async Task Should_Set_GeoInfo_In_HttpContext()
        {
            var json = "{\"ip\":\"5.6.7.8\",\"city\":\"Delhi\",\"region\":\"DL\"}";
            var httpClient = CreateHttpClient(json);

            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var middleware = new GeoLocationMiddleware(next: ctx => Task.CompletedTask, factory.Object);

            var ctx = new DefaultHttpContext();
            ctx.Connection.RemoteIpAddress = IPAddress.Parse("5.6.7.8");

            await middleware.InvokeAsync(ctx);

            var geo = ctx.Items["GeoInfo"] as GeoInfo;

            Assert.NotNull(geo);
            Assert.Equal("Delhi", geo.city);
            Assert.Equal("DL", geo.region);
        }
    }
}
