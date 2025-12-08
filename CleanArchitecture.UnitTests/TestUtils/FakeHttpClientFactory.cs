using System.Net.Http;

namespace CleanArchitecture.UnitTests.TestUtils
{
    public class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new FakeGeoHttpHandler());
        }
    }
}
