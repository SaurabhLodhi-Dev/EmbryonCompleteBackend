namespace CleanArchitecture.Application.DTOs.Appointments
{
    public record AppointmentSlotDto(
        Guid Id,
        DateTime SlotDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        bool? IsBooked
    );
}
