using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.DTOs.Contact
{
    public record ContactSubmissionDto(
        Guid Id,
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        string? PhoneCountryCode,
        string? Country,
        string? State,
        string? City,
        string? Subject,
        string? Message,
        string? IpAddress,
        DateTime CreatedAt
    );
}
