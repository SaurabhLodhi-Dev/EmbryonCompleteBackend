using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    /// <summary>
    /// Middleware to detect client IP and fetch geo-location using ipapi.co.
    /// Automatically handles localhost, proxies, IPv6, errors & fallback.
    /// </summary>
    public class GeoLocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;

        public GeoLocationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _httpClient = httpClientFactory.CreateClient("GeoClient");

        }

        public async Task InvokeAsync(HttpContext context)
        {
            GeoInfo geo = new GeoInfo();

            try
            {
                string ip = GetClientIp(context);

                // Convert IPv6 localhost to IPv4
                if (ip == "::1" || ip == "0:0:0:0:0:0:0:1")
                    ip = "127.0.0.1";

                geo.ip = ip; // Always store the IP

                // Query Geo-IP service
                var url = $"https://ipapi.co/{ip}/json/";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiGeo = JsonSerializer.Deserialize<GeoInfo>(json);

                    if (apiGeo != null)
                    {
                        apiGeo.ip = ip; // Override to be safe
                        geo = apiGeo;
                    }
                }
            }
            catch
            {
                // Swallow errors – never block request
            }

            // Make GeoInfo available to controllers
            context.Items["GeoInfo"] = geo;

            await _next(context);
        }

        private string GetClientIp(HttpContext context)
        {
            // For load balancers, reverse proxies
            string? forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            // Standard client IP
            return context.Connection.RemoteIpAddress?
                .MapToIPv4()
                .ToString() ?? "0.0.0.0";
        }
    }

    /// <summary>
    /// Strongly typed geo result model.
    /// </summary>
    public class GeoInfo
    {
        public string? ip { get; set; }
        public string? city { get; set; }
        public string? region { get; set; }
        public string? country { get; set; }
        public string? country_name { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
    }
}
