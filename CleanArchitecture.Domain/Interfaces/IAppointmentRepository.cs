using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<Appointment> GetByIdAsync(Guid id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment> AddAsync(Appointment entity);
    }
}
