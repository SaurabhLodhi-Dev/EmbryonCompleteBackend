namespace CleanArchitecture.Application.DTOs.Contact
{
    public record CreateContactSubmissionDto(
     string? FirstName,
     string? LastName,
     string? Email,
     string? Phone,
     string? PhoneCountryCode,
     Guid? CountryId,
     string? State,
     string? City,
     string? Subject,
     string? Message,
     string? CaptchaToken,
     string? IpAddress
 );

}
