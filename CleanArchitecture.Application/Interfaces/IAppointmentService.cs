using CleanArchitecture.Application.DTOs.Appointments;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AppointmentDto>> GetAllAsync();
        Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    }
}
