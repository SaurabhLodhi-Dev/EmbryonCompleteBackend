using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class FakeGeoHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancel)
    {
        var json = """
        {
            "ip": "1.1.1.1",
            "city": "Indore",
            "region": "MP",
            "country": "India"
        }
        """;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
    }
}
