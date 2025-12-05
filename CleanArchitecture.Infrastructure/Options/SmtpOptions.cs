namespace CleanArchitecture.Infrastructure.Options
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public bool UseStartTls { get; set; } = false;

        public string? UserName { get; set; }
        public string? Password { get; set; }

        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = "Embryon System";
        public string AdminEmail { get; set; } = "";
    }
}
