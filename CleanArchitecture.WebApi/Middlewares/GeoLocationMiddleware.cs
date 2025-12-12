//using Microsoft.Extensions.Caching.Memory;
//using System.Net;
//using System.Text.Json;

//namespace CleanArchitecture.WebApi.Middlewares
//{
//    public class GeoLocationMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<GeoLocationMiddleware> _logger;
//        private static DateTime? GeoServiceDownUntil = null;
//        private readonly object _geoLock = new();


//        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

//        public GeoLocationMiddleware(
//            RequestDelegate next,
//            IHttpClientFactory httpClientFactory,
//            IMemoryCache cache,
//            ILogger<GeoLocationMiddleware> logger)
//        {
//            _next = next;
//            _httpClientFactory = httpClientFactory;
//            _cache = cache;
//            _logger = logger;
//        }

//        public async Task InvokeAsync(HttpContext context)
//        {
//            GeoInfo geo = new GeoInfo();

//            try
//            {
//                // 1️⃣ Extract REAL client IP from proxies
//                string ip = GetRealClientIp(context);
//                geo.ip = ip;

//                _logger.LogInformation("📡 Client IP detected: {IP}", ip);

//                // Extract user agent
//                geo.user_agent = context.Request.Headers["User-Agent"].ToString();

//                // 2️⃣ Skip lookup for private/local IPs
//                if (IsLocalOrPrivateIp(ip))
//                {
//                    _logger.LogWarning("Local/private IP detected. Skipping geo lookup. IP: {IP}", ip);

//                    geo.city = "Local";
//                    geo.region = "Development";
//                    geo.country = "XX";
//                    geo.country_name = "Local Development";
//                }
//                else
//                {
//                    string cacheKey = $"geo_{ip}";

//                    // 3️⃣ Try cache first
//                    if (!_cache.TryGetValue(cacheKey, out GeoInfo? cached))
//                    {
//                        _logger.LogInformation("🌍 Fetching geo data for IP: {IP}", ip);

//                        cached = await FetchGeoDataAsync(ip);

//                        if (cached != null)
//                        {
//                            _cache.Set(cacheKey, cached, CacheDuration);
//                            _logger.LogInformation("Geo cached for IP: {IP}", ip);
//                        }
//                    }

//                    if (cached != null)
//                    {
//                        geo = cached;
//                        geo.ip = ip;
//                        geo.user_agent = context.Request.Headers["User-Agent"].ToString();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Geo middleware error.");
//            }

//            // Make available inside controllers
//            context.Items["GeoInfo"] = geo;

//            await _next(context);
//        }

//        // --------------------------------------------------------------------
//        // REAL PRODUCTION GRADE IP EXTRACTION
//        // --------------------------------------------------------------------
//        private string GetRealClientIp(HttpContext context)
//        {
//            string[] headers =
//            {
//                "CF-Connecting-IP",    // Cloudflare
//                "True-Client-IP",      // Cloudflare enterprise
//                "X-Real-IP",           // Nginx
//                "X-Client-IP",         // Azure, AWS
//                "X-Forwarded-For",     // standard proxy header
//            };

//            foreach (var header in headers)
//            {
//                if (context.Request.Headers.TryGetValue(header, out var value))
//                {
//                    var raw = value.ToString();

//                    // X-Forwarded-For: first IP is original client
//                    var ip = raw.Split(',')[0].Trim();

//                    if (IPAddress.TryParse(ip, out _))
//                    {
//                        _logger.LogInformation("📌 Client IP from header {Header}: {IP}", header, ip);
//                        return ip;
//                    }
//                }
//            }

//            // FINAL fallback — often shows proxy IP
//            var remote = context.Connection.RemoteIpAddress;
//            if (remote != null)
//            {
//                if (remote.IsIPv4MappedToIPv6)
//                    remote = remote.MapToIPv4();

//                string ip = remote.ToString();
//                return ip == "::1" ? "127.0.0.1" : ip;
//            }

//            return "0.0.0.0";
//        }

//        // --------------------------------------------------------------------
//        // GEO LOOKUP API (ipwho + ip-api fallback)
//        // --------------------------------------------------------------------
//        private async Task<GeoInfo?> FetchGeoDataAsync(string ip)
//        {
//            // Primary lookup
//            var fromWho = await FetchFromIpWhoAsync(ip);
//            if (fromWho != null) return fromWho;

//            // Fallback lookup
//            return await FetchFromIpApiAsync(ip);
//        }

//        private async Task<GeoInfo?> FetchFromIpWhoAsync(string ip)
//        {
//            try
//            {
//                var client = _httpClientFactory.CreateClient("GeoClient");
//                var res = await client.GetAsync($"https://ipwho.is/{ip}");

//                if (!res.IsSuccessStatusCode) return null;

//                var json = await res.Content.ReadAsStringAsync();
//                var response = JsonSerializer.Deserialize<IpWhoResponse>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (response?.Success != true) return null;

//                return new GeoInfo
//                {
//                    ip = ip,
//                    city = response.City,
//                    region = response.Region,
//                    country = response.Country_Code,
//                    country_name = response.Country,
//                    latitude = response.Latitude,
//                    longitude = response.Longitude
//                };
//            }
//            catch
//            {
//                return null;
//            }
//        }

//        private async Task<GeoInfo?> FetchFromIpApiAsync(string ip)
//        {
//            try
//            {
//                var client = _httpClientFactory.CreateClient("GeoClient");
//                var res = await client.GetAsync($"http://ip-api.com/json/{ip}");

//                if (!res.IsSuccessStatusCode) return null;

//                var json = await res.Content.ReadAsStringAsync();
//                var response = JsonSerializer.Deserialize<IpApiResponse>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (response?.Status != "success") return null;

//                return new GeoInfo
//                {
//                    ip = ip,
//                    city = response.City,
//                    region = response.RegionName,
//                    country = response.CountryCode,
//                    country_name = response.Country,
//                    latitude = response.Lat,
//                    longitude = response.Lon
//                };
//            }
//            catch
//            {
//                return null;
//            }
//        }

//        // --------------------------------------------------------------------
//        // Helpers
//        // --------------------------------------------------------------------
//        private bool IsLocalOrPrivateIp(string ipString)
//        {
//            if (!IPAddress.TryParse(ipString, out var ip))
//                return true;

//            byte[] bytes = ip.GetAddressBytes();

//            if (bytes.Length == 4)
//            {
//                if (bytes[0] == 10) return true;
//                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
//                if (bytes[0] == 192 && bytes[1] == 168) return true;
//                if (bytes[0] == 127) return true;
//            }

//            return false;
//        }
//    }

//    // --------------------------------------------------------------------
//    // Models
//    // --------------------------------------------------------------------
//    public class IpWhoResponse
//    {
//        public bool Success { get; set; }
//        public string? City { get; set; }
//        public string? Region { get; set; }
//        public string? Country { get; set; }
//        public string? Country_Code { get; set; }
//        public double? Latitude { get; set; }
//        public double? Longitude { get; set; }
//    }

//    public class IpApiResponse
//    {
//        public string? Status { get; set; }
//        public string? Country { get; set; }
//        public string? CountryCode { get; set; }
//        public string? RegionName { get; set; }
//        public string? City { get; set; }
//        public double? Lat { get; set; }
//        public double? Lon { get; set; }
//    }

//    public class GeoInfo
//    {
//        public string? ip { get; set; }
//        public string? city { get; set; }
//        public string? region { get; set; }
//        public string? country { get; set; }
//        public string? country_name { get; set; }
//        public double? latitude { get; set; }
//        public double? longitude { get; set; }
//        public string? user_agent { get; set; }
//    }
//}



using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;

namespace CleanArchitecture.WebApi.Middlewares
{
    public class GeoLocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GeoLocationMiddleware> _logger;

        // Circuit-breaker
        private static DateTime? GeoServiceDownUntil = null;
        private readonly object _geoLock = new();

        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

        public GeoLocationMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<GeoLocationMiddleware> logger)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            GeoInfo geo = new GeoInfo();

            try
            {
                // Extract real client IP
                string ip = GetRealClientIp(context);
                geo.ip = ip;
                geo.user_agent = context.Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("📡 Client IP detected: {IP}", ip);

                if (IsLocalOrPrivateIp(ip))
                {
                    _logger.LogWarning("Local/private IP detected. Skipping geo lookup. IP: {IP}", ip);

                    geo.city = "Local";
                    geo.region = "Development";
                    geo.country = "XX";
                    geo.country_name = "Local Development";
                }
                else
                {
                    string cacheKey = $"geo_{ip}";

                    if (!_cache.TryGetValue(cacheKey, out GeoInfo? cached))
                    {
                        // Circuit-breaker evaluation
                        bool skipLookup = false;

                        lock (_geoLock)
                        {
                            if (GeoServiceDownUntil.HasValue &&
                                GeoServiceDownUntil.Value > DateTime.UtcNow)
                            {
                                skipLookup = true;
                                _logger.LogWarning(
                                    "Geo service down until {Until}. Skipping external lookup.",
                                    GeoServiceDownUntil.Value);
                            }
                        }

                        if (!skipLookup)
                        {
                            cached = await FetchGeoDataAsync(ip, context.RequestAborted);

                            // If service failed → activate circuit breaker
                            if (cached == null)
                            {
                                lock (_geoLock)
                                {
                                    _logger.LogError("Geo lookup failed. Marking service down for 5 minutes.");
                                    GeoServiceDownUntil = DateTime.UtcNow.AddMinutes(5);
                                }
                            }
                            else
                            {
                                _cache.Set(cacheKey, cached, CacheDuration);
                            }
                        }
                    }

                    if (cached != null)
                    {
                        geo = cached;
                        geo.ip = ip;
                        geo.user_agent = context.Request.Headers["User-Agent"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geo middleware error.");
            }

            context.Items["GeoInfo"] = geo;

            await _next(context);
        }

        // ---------------------------------------------------------------------
        // Real Client IP Extraction
        // ---------------------------------------------------------------------
        private string GetRealClientIp(HttpContext context)
        {
            string[] headers =
            {
                "CF-Connecting-IP",
                "True-Client-IP",
                "X-Real-IP",
                "X-Client-IP",
                "X-Forwarded-For",
            };

            foreach (var header in headers)
            {
                if (context.Request.Headers.TryGetValue(header, out var value))
                {
                    var raw = value.ToString();
                    var ip = raw.Split(',')[0].Trim();

                    if (IPAddress.TryParse(ip, out _))
                    {
                        _logger.LogInformation("📌 Client IP from header {Header}: {IP}", header, ip);
                        return ip;
                    }
                }
            }

            // Fallback
            var remote = context.Connection.RemoteIpAddress;
            if (remote != null)
            {
                if (remote.IsIPv4MappedToIPv6)
                    remote = remote.MapToIPv4();

                string ip = remote.ToString();
                return ip == "::1" ? "127.0.0.1" : ip;
            }

            return "0.0.0.0";
        }

        // ---------------------------------------------------------------------
        // GEO LOOKUP with Circuit Breaker + Cancellation
        // ---------------------------------------------------------------------
        private async Task<GeoInfo?> FetchGeoDataAsync(string ip, CancellationToken ct)
        {
            var fromWho = await FetchFromIpWhoAsync(ip, ct);
            if (fromWho != null) return fromWho;

            return await FetchFromIpApiAsync(ip, ct);
        }

        private async Task<GeoInfo?> FetchFromIpWhoAsync(string ip, CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoClient");

                var res = await client.GetAsync(
                    $"https://ipwho.is/{ip}",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                if (!res.IsSuccessStatusCode) return null;

                var json = await res.Content.ReadAsStringAsync(ct);

                var response = JsonSerializer.Deserialize<IpWhoResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response?.Success != true) return null;

                return new GeoInfo
                {
                    ip = ip,
                    city = response.City,
                    region = response.Region,
                    country = response.Country_Code,
                    country_name = response.Country,
                    latitude = response.Latitude,
                    longitude = response.Longitude
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IPWho lookup failed.");
                return null;
            }
        }

        private async Task<GeoInfo?> FetchFromIpApiAsync(string ip, CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoClient");

                var res = await client.GetAsync(
                    $"http://ip-api.com/json/{ip}",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                if (!res.IsSuccessStatusCode) return null;

                var json = await res.Content.ReadAsStringAsync(ct);

                var response = JsonSerializer.Deserialize<IpApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (response?.Status != "success") return null;

                return new GeoInfo
                {
                    ip = ip,
                    city = response.City,
                    region = response.RegionName,
                    country = response.CountryCode,
                    country_name = response.Country,
                    latitude = response.Lat,
                    longitude = response.Lon
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IP-API lookup failed.");
                return null;
            }
        }

        // ---------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------
        private bool IsLocalOrPrivateIp(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
                return true;

            byte[] bytes = ip.GetAddressBytes();

            if (bytes.Length == 4)
            {
                if (bytes[0] == 10) return true;
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
                if (bytes[0] == 192 && bytes[1] == 168) return true;
                if (bytes[0] == 127) return true;
            }

            return false;
        }
    }

    // Models
    public class IpWhoResponse
    {
        public bool Success { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Country_Code { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
    }

    public class GeoInfo
    {
        public string? ip { get; set; }
        public string? city { get; set; }
        public string? region { get; set; }
        public string? country { get; set; }
        public string? country_name { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? user_agent { get; set; }
    }
}
