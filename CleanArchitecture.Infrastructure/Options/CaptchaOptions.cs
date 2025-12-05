namespace CleanArchitecture.Infrastructure.Options
{
    public class CaptchaOptions
    {
        public bool Enabled { get; set; }
        public string SiteKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public string VerifyUrl { get; set; } = "";
    }
}
