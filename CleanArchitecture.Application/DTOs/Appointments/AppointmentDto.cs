namespace CleanArchitecture.Application.DTOs.Appointments
{
    public record AppointmentDto(
         Guid Id,
         string? FirstName,
         string? LastName,
         string? Email,
         string? Description,
         string? AppointmentTypeName,
         DateTime SlotDate,
         TimeSpan StartTime,
         TimeSpan EndTime,
         string? IpAddress,
         string? UserAgent,
         DateTime CreatedAt
     );

}
