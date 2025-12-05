using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace CleanArchitecture.Infrastructure.Services
{
    public class CaptchaValidator : ICaptchaValidator
    {
        private readonly HttpClient _http;
        private readonly CaptchaOptions _options;

        public CaptchaValidator(HttpClient http, IOptions<CaptchaOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<bool> ValidateAsync(string token, string ip)
        {
            if (!_options.Enabled)
                return true; // 🔥 DEVELOPMENT MODE — ALWAYS PASS

            var response = await _http.PostAsync(_options.VerifyUrl,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", _options.SecretKey },
                    { "response", token },
                    { "remoteip", ip }
                }));

            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
            return result?.Success ?? false;
        }
    }

    public class TurnstileResponse
    {
        public bool Success { get; set; }
        public List<string>? ErrorCodes { get; set; }
    }
}
