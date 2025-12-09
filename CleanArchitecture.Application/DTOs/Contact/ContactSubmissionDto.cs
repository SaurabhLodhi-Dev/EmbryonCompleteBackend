namespace CleanArchitecture.Application.DTOs.Contact
{
    /// <summary>
    /// DTO returned when retrieving contact submissions.
    /// Contains all contact details plus metadata.
    /// </summary>
    public record ContactSubmissionDto(
        Guid Id,
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        string? PhoneCountryCode,
        string? CountryName,
        string? State,
        string? City,
        string? Subject,
        string? Message,
        string? IpAddress,
        string? UserAgent,
        DateTime CreatedAt,
        double? Latitude,
double? Longitude

    );
}