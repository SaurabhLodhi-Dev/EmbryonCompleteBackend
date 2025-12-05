namespace CleanArchitecture.Application.DTOs.Appointments
{
    public record CreateAppointmentSlotDto(
        DateTime SlotDate,
        TimeSpan StartTime,
        TimeSpan EndTime
    );
}
