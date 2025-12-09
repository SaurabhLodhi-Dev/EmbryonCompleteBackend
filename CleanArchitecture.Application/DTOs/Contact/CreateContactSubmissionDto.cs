namespace CleanArchitecture.Application.DTOs.Contact
{
    /// <summary>
    /// DTO for creating a new contact submission.
    /// Client sends: Name, Email, Phone, Message, CAPTCHA token
    /// Server adds: IP Address, UserAgent (from middleware)
    /// </summary>
    public record CreateContactSubmissionDto
    {
        // ==========================================
        // CLIENT-PROVIDED FIELDS
        // ==========================================

        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? PhoneCountryCode { get; init; }
        // CHANGED HERE
        public string? CountryName { get; init; }
        public string? State { get; init; }
        public string? City { get; init; }
        public string? Subject { get; init; }
        public string? Message { get; init; }

        /// <summary>
        /// reCAPTCHA v3 token from client (required for bot protection)
        /// </summary>
        public string? CaptchaToken { get; init; }

        // ==========================================
        // SERVER-ADDED FIELDS (from middleware)
        // ==========================================

        /// <summary>
        /// Client IP address (set by GeoLocationMiddleware)
        /// </summary>
        public string? IpAddress { get; init; }

        /// <summary>
        /// Client User-Agent (set by GeoLocationMiddleware)
        /// </summary>
        public string? UserAgent { get; init; }

        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
    }
}