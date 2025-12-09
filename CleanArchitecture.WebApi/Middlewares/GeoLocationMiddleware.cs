////using System.Text.Json;

////namespace CleanArchitecture.WebApi.Middlewares
////{
////    /// <summary>
////    /// Middleware to detect client IP and fetch geo-location using ipapi.co.
////    /// Automatically handles localhost, proxies, IPv6, errors & fallback.
////    /// </summary>
////    public class GeoLocationMiddleware
////    {
////        private readonly RequestDelegate _next;
////        private readonly HttpClient _httpClient;

////        public GeoLocationMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
////        {
////            _next = next;
////            _httpClient = httpClientFactory.CreateClient("GeoClient");

////        }

////        public async Task InvokeAsync(HttpContext context)
////        {
////            GeoInfo geo = new GeoInfo();

////            try
////            {
////                string ip = GetClientIp(context);

////                // Convert IPv6 localhost to IPv4
////                if (ip == "::1" || ip == "0:0:0:0:0:0:0:1")
////                    ip = "127.0.0.1";

////                geo.ip = ip; // Always store the IP

////                // Query Geo-IP service
////                var url = $"https://ipapi.co/{ip}/json/";
////                var response = await _httpClient.GetAsync(url);

////                if (response.IsSuccessStatusCode)
////                {
////                    var json = await response.Content.ReadAsStringAsync();
////                    var apiGeo = JsonSerializer.Deserialize<GeoInfo>(json);

////                    if (apiGeo != null)
////                    {
////                        apiGeo.ip = ip; // Override to be safe
////                        geo = apiGeo;
////                    }
////                }
////            }
////            catch
////            {
////                // Swallow errors – never block request
////            }

////            // Make GeoInfo available to controllers
////            context.Items["GeoInfo"] = geo;

////            await _next(context);
////        }

////        private string GetClientIp(HttpContext context)
////        {
////            // For load balancers, reverse proxies
////            string? forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
////            if (!string.IsNullOrWhiteSpace(forwardedFor))
////                return forwardedFor.Split(',')[0].Trim();

////            // Standard client IP
////            return context.Connection.RemoteIpAddress?
////                .MapToIPv4()
////                .ToString() ?? "0.0.0.0";
////        }
////    }

////    /// <summary>
////    /// Strongly typed geo result model.
////    /// </summary>
////    public class GeoInfo
////    {
////        public string? ip { get; set; }
////        public string? city { get; set; }
////        public string? region { get; set; }
////        public string? country { get; set; }
////        public string? country_name { get; set; }
////        public double? latitude { get; set; }
////        public double? longitude { get; set; }
////    }
////}


//using Microsoft.Extensions.Caching.Memory;
//using System.Net;
//using System.Text.Json;

//namespace CleanArchitecture.WebApi.Middlewares
//{
//    /// <summary>
//    /// Production-ready middleware to detect client IP and fetch geo-location.
//    /// Features: caching, rate limiting protection, proper error handling, localhost detection.
//    /// </summary>
//    public class GeoLocationMiddleware
//    {
//        private readonly RequestDelegate _next;
//        private readonly IHttpClientFactory _httpClientFactory;
//        private readonly IMemoryCache _cache;
//        private readonly ILogger<GeoLocationMiddleware> _logger;

//        // Cache geo data for 24 hours per IP
//        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

//        // Known private/local IP ranges
//        private static readonly HashSet<string> LocalIpAddresses = new()
//        {
//            "127.0.0.1", "::1", "0.0.0.0", "localhost"
//        };

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
//            var geo = new GeoInfo();

//            try
//            {
//                // Extract real client IP
//                string ip = GetClientIp(context);
//                geo.ip = ip;

//                // Extract User Agent
//                geo.user_agent = context.Request.Headers["User-Agent"].ToString();

//                // Check if IP is local/private
//                if (IsLocalOrPrivateIp(ip))
//                {
//                    _logger.LogDebug("Local IP detected: {IP}. Skipping geo lookup.", ip);
//                    geo.city = "Local";
//                    geo.region = "Development";
//                    geo.country = "XX";
//                    geo.country_name = "Local Development";
//                }
//                else
//                {
//                    // Try to get from cache first
//                    string cacheKey = $"geo_{ip}";

//                    if (!_cache.TryGetValue(cacheKey, out GeoInfo? cachedGeo))
//                    {
//                        // Fetch from API
//                        cachedGeo = await FetchGeoDataAsync(ip);

//                        if (cachedGeo != null)
//                        {
//                            // Cache the result
//                            _cache.Set(cacheKey, cachedGeo, CacheDuration);
//                            _logger.LogInformation("Geo data cached for IP: {IP}", ip);
//                        }
//                    }
//                    else
//                    {
//                        _logger.LogDebug("Geo data retrieved from cache for IP: {IP}", ip);
//                    }

//                    if (cachedGeo != null)
//                    {
//                        geo = cachedGeo;
//                        geo.ip = ip; // Ensure IP is set
//                        geo.user_agent = context.Request.Headers["User-Agent"].ToString();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log but never block the request
//                _logger.LogError(ex, "Error in GeoLocationMiddleware for IP: {IP}", geo.ip);
//            }

//            // Make GeoInfo available to controllers
//            context.Items["GeoInfo"] = geo;

//            await _next(context);
//        }

//        /// <summary>
//        /// Fetches geo data from ipwho.is API with fallback to ip-api.com
//        /// </summary>
//        private async Task<GeoInfo?> FetchGeoDataAsync(string ip)
//        {
//            try
//            {
//                // Primary: ipwho.is (free, 10k requests/month)
//                var geo = await FetchFromIpWhoAsync(ip);
//                if (geo != null) return geo;

//                // Fallback: ip-api.com (free, 45 requests/minute)
//                _logger.LogWarning("ipwho.is failed, trying fallback for IP: {IP}", ip);
//                return await FetchFromIpApiAsync(ip);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "All geo API attempts failed for IP: {IP}", ip);
//                return null;
//            }
//        }

//        /// <summary>
//        /// Primary API: ipwho.is
//        /// </summary>
//        private async Task<GeoInfo?> FetchFromIpWhoAsync(string ip)
//        {
//            try
//            {
//                var client = _httpClientFactory.CreateClient("GeoClient");
//                var url = $"https://ipwho.is/{ip}";

//                var response = await client.GetAsync(url);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning("ipwho.is returned {StatusCode} for IP: {IP}",
//                        response.StatusCode, ip);
//                    return null;
//                }

//                var json = await response.Content.ReadAsStringAsync();
//                var apiResponse = JsonSerializer.Deserialize<IpWhoResponse>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (apiResponse?.Success != true)
//                {
//                    _logger.LogWarning("ipwho.is returned success=false for IP: {IP}", ip);
//                    return null;
//                }

//                return new GeoInfo
//                {
//                    ip = ip,
//                    city = apiResponse.City,
//                    region = apiResponse.Region,
//                    country = apiResponse.Country_Code,
//                    country_name = apiResponse.Country,
//                    latitude = apiResponse.Latitude,
//                    longitude = apiResponse.Longitude
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error fetching from ipwho.is for IP: {IP}", ip);
//                return null;
//            }
//        }

//        /// <summary>
//        /// Fallback API: ip-api.com
//        /// </summary>
//        private async Task<GeoInfo?> FetchFromIpApiAsync(string ip)
//        {
//            try
//            {
//                var client = _httpClientFactory.CreateClient("GeoClient");
//                var url = $"http://ip-api.com/json/{ip}";

//                var response = await client.GetAsync(url);

//                if (!response.IsSuccessStatusCode)
//                {
//                    _logger.LogWarning("ip-api.com returned {StatusCode} for IP: {IP}",
//                        response.StatusCode, ip);
//                    return null;
//                }

//                var json = await response.Content.ReadAsStringAsync();
//                var apiResponse = JsonSerializer.Deserialize<IpApiResponse>(json, new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                });

//                if (apiResponse?.Status != "success")
//                {
//                    _logger.LogWarning("ip-api.com returned status={Status} for IP: {IP}",
//                        apiResponse?.Status, ip);
//                    return null;
//                }

//                return new GeoInfo
//                {
//                    ip = ip,
//                    city = apiResponse.City,
//                    region = apiResponse.RegionName,
//                    country = apiResponse.CountryCode,
//                    country_name = apiResponse.Country,
//                    latitude = apiResponse.Lat,
//                    longitude = apiResponse.Lon
//                };
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error fetching from ip-api.com for IP: {IP}", ip);
//                return null;
//            }
//        }

//        /// <summary>
//        /// Extracts the real client IP from the request, handling proxies and load balancers.
//        /// </summary>
//        private string GetClientIp(HttpContext context)
//        {
//            // Check for common proxy headers (in order of preference)
//            var headers = new[]
//            {
//                "CF-Connecting-IP",      // Cloudflare
//                "True-Client-IP",        // Cloudflare Enterprise
//                "X-Real-IP",             // Nginx
//                "X-Forwarded-For",       // Standard
//                "X-Cluster-Client-IP",   // Rackspace, Riverbed
//                "Forwarded"              // RFC 7239
//            };

//            foreach (var header in headers)
//            {
//                var value = context.Request.Headers[header].ToString();
//                if (!string.IsNullOrWhiteSpace(value))
//                {
//                    // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2)
//                    // Take the first one (original client)
//                    var ip = value.Split(',')[0].Trim();

//                    if (IsValidIpAddress(ip))
//                    {
//                        _logger.LogDebug("IP extracted from {Header}: {IP}", header, ip);
//                        return ip;
//                    }
//                }
//            }

//            // Fallback to connection remote IP
//            var remoteIp = context.Connection.RemoteIpAddress;

//            if (remoteIp != null)
//            {
//                // Convert IPv6 localhost to IPv4
//                if (remoteIp.IsIPv4MappedToIPv6)
//                {
//                    remoteIp = remoteIp.MapToIPv4();
//                }

//                var ipString = remoteIp.ToString();

//                // Handle IPv6 localhost
//                if (ipString == "::1")
//                {
//                    ipString = "127.0.0.1";
//                }

//                return ipString;
//            }

//            return "0.0.0.0"; // Unknown
//        }

//        /// <summary>
//        /// Validates if a string is a valid IP address.
//        /// </summary>
//        private bool IsValidIpAddress(string ip)
//        {
//            return IPAddress.TryParse(ip, out _);
//        }

//        /// <summary>
//        /// Checks if an IP is local or private (non-routable).
//        /// </summary>
//        private bool IsLocalOrPrivateIp(string ipString)
//        {
//            if (LocalIpAddresses.Contains(ipString))
//                return true;

//            if (!IPAddress.TryParse(ipString, out var ip))
//                return true;

//            // Check for private IP ranges
//            var bytes = ip.GetAddressBytes();

//            if (bytes.Length == 4) // IPv4
//            {
//                // 10.0.0.0/8
//                if (bytes[0] == 10)
//                    return true;

//                // 172.16.0.0/12
//                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
//                    return true;

//                // 192.168.0.0/16
//                if (bytes[0] == 192 && bytes[1] == 168)
//                    return true;

//                // 127.0.0.0/8 (loopback)
//                if (bytes[0] == 127)
//                    return true;
//            }

//            return false;
//        }
//    }

//    /// <summary>
//    /// Response model for ipwho.is API
//    /// </summary>
//    public class IpWhoResponse
//    {
//        public bool Success { get; set; }
//        public string? Ip { get; set; }
//        public string? City { get; set; }
//        public string? Region { get; set; }
//        public string? Country { get; set; }
//        public string? Country_Code { get; set; }
//        public double? Latitude { get; set; }
//        public double? Longitude { get; set; }
//    }

//    /// <summary>
//    /// Response model for ip-api.com API
//    /// </summary>
//    public class IpApiResponse
//    {
//        public string? Status { get; set; }
//        public string? Country { get; set; }
//        public string? CountryCode { get; set; }
//        public string? Region { get; set; }
//        public string? RegionName { get; set; }
//        public string? City { get; set; }
//        public double? Lat { get; set; }
//        public double? Lon { get; set; }
//    }

//    /// <summary>
//    /// Strongly typed geo result model.
//    /// </summary>
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
                // 1️⃣ Extract REAL client IP from proxies
                string ip = GetRealClientIp(context);
                geo.ip = ip;

                _logger.LogInformation("📡 Client IP detected: {IP}", ip);

                // Extract user agent
                geo.user_agent = context.Request.Headers["User-Agent"].ToString();

                // 2️⃣ Skip lookup for private/local IPs
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

                    // 3️⃣ Try cache first
                    if (!_cache.TryGetValue(cacheKey, out GeoInfo? cached))
                    {
                        _logger.LogInformation("🌍 Fetching geo data for IP: {IP}", ip);

                        cached = await FetchGeoDataAsync(ip);

                        if (cached != null)
                        {
                            _cache.Set(cacheKey, cached, CacheDuration);
                            _logger.LogInformation("Geo cached for IP: {IP}", ip);
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

            // Make available inside controllers
            context.Items["GeoInfo"] = geo;

            await _next(context);
        }

        // --------------------------------------------------------------------
        // REAL PRODUCTION GRADE IP EXTRACTION
        // --------------------------------------------------------------------
        private string GetRealClientIp(HttpContext context)
        {
            string[] headers =
            {
                "CF-Connecting-IP",    // Cloudflare
                "True-Client-IP",      // Cloudflare enterprise
                "X-Real-IP",           // Nginx
                "X-Client-IP",         // Azure, AWS
                "X-Forwarded-For",     // standard proxy header
            };

            foreach (var header in headers)
            {
                if (context.Request.Headers.TryGetValue(header, out var value))
                {
                    var raw = value.ToString();

                    // X-Forwarded-For: first IP is original client
                    var ip = raw.Split(',')[0].Trim();

                    if (IPAddress.TryParse(ip, out _))
                    {
                        _logger.LogInformation("📌 Client IP from header {Header}: {IP}", header, ip);
                        return ip;
                    }
                }
            }

            // FINAL fallback — often shows proxy IP
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

        // --------------------------------------------------------------------
        // GEO LOOKUP API (ipwho + ip-api fallback)
        // --------------------------------------------------------------------
        private async Task<GeoInfo?> FetchGeoDataAsync(string ip)
        {
            // Primary lookup
            var fromWho = await FetchFromIpWhoAsync(ip);
            if (fromWho != null) return fromWho;

            // Fallback lookup
            return await FetchFromIpApiAsync(ip);
        }

        private async Task<GeoInfo?> FetchFromIpWhoAsync(string ip)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoClient");
                var res = await client.GetAsync($"https://ipwho.is/{ip}");

                if (!res.IsSuccessStatusCode) return null;

                var json = await res.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<IpWhoResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

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
            catch
            {
                return null;
            }
        }

        private async Task<GeoInfo?> FetchFromIpApiAsync(string ip)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeoClient");
                var res = await client.GetAsync($"http://ip-api.com/json/{ip}");

                if (!res.IsSuccessStatusCode) return null;

                var json = await res.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<IpApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

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
            catch
            {
                return null;
            }
        }

        // --------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------
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

    // --------------------------------------------------------------------
    // Models
    // --------------------------------------------------------------------
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
