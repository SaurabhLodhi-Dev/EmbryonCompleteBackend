////using CleanArchitecture.Application.Interfaces;
////using CleanArchitecture.Infrastructure.Options;
////using Microsoft.Extensions.Options;
////using System.Net.Http.Json;

////namespace CleanArchitecture.Infrastructure.Services
////{
////    public class CaptchaValidator : ICaptchaValidator
////    {
////        private readonly HttpClient _http;
////        private readonly CaptchaOptions _options;

////        public CaptchaValidator(HttpClient http, IOptions<CaptchaOptions> options)
////        {
////            _http = http;
////            _options = options.Value;
////        }

////        public async Task<bool> ValidateAsync(string token, string ip)
////        {
////            if (!_options.Enabled)
////                return true; // 🔥 DEVELOPMENT MODE — ALWAYS PASS

////            var response = await _http.PostAsync(_options.VerifyUrl,
////                new FormUrlEncodedContent(new Dictionary<string, string>
////                {
////                    { "secret", _options.SecretKey },
////                    { "response", token },
////                    { "remoteip", ip }
////                }));

////            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
////            return result?.Success ?? false;
////        }
////    }

////    public class TurnstileResponse
////    {
////        public bool Success { get; set; }
////        public List<string>? ErrorCodes { get; set; }
////    }
////}

//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.Infrastructure.Options;
//using Microsoft.Extensions.Options;
//using System.Net.Http.Json;

//namespace CleanArchitecture.Infrastructure.Services
//{
//    public class CaptchaValidator : ICaptchaValidator
//    {
//        private readonly HttpClient _http;
//        private readonly CaptchaOptions _options;

//        public CaptchaValidator(HttpClient http, IOptions<CaptchaOptions> options)
//        {
//            _http = http;
//            _options = options.Value;
//        }

//        public async Task<bool> ValidateAsync(string token, string ip)
//        {
//            if (!_options.Enabled)
//                return true; // 🔥 DEVELOPMENT MODE — always pass

//            var content = new FormUrlEncodedContent(new Dictionary<string, string>
//            {
//                { "secret", _options.SecretKey },
//                { "response", token },
//                { "remoteip", ip }
//            });

//            var response = await _http.PostAsync(_options.VerifyUrl, content);
//            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();

//            if (result == null)
//                return false;

//            if (!result.Success)
//                return false;

//            // Optional: enforce score threshold if using reCAPTCHA v3
//            if (_options.ScoreThreshold.HasValue && result.Score.HasValue && result.Score < _options.ScoreThreshold.Value)
//                return false;

//            return true;
//        }
//    }

//    public class TurnstileResponse
//    {
//        public bool Success { get; set; }
//        public double? Score { get; set; } // for reCAPTCHA v3
//        public List<string>? ErrorCodes { get; set; }
//    }
//}




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
                return true; // 🔥 DEVELOPMENT MODE — always pass

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _options.SecretKey },
                { "response", token },
                { "remoteip", ip }
            });

            var response = await _http.PostAsync(_options.VerifyUrl, content);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();

            if (result == null)
                return false;

            if (!result.Success)
                return false;

            // ✅ Correct score check (no HasValue on double)
            if (result.Score is double score && score < _options.ScoreThreshold)
                return false;

            return true;
        }
    }

    public class TurnstileResponse
    {
        public bool Success { get; set; }
        public double? Score { get; set; } // Cloudflare optional score
        public List<string>? ErrorCodes { get; set; }
    }
}
