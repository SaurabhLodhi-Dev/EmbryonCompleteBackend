namespace CleanArchitecture.Application.DTOs.Appointments
{
    public record CreateAppointmentDto(
        string? FirstName,
        string? LastName,
        string? Email,
        string? Description,
        Guid? AppointmentTypeId,
        Guid? SlotId,
        string? IpAddress,
        string? UserAgent
    );

}
