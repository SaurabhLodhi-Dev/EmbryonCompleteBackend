using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface IAppointmentTypeRepository
    {
        Task<AppointmentType?> GetByIdAsync(Guid id);
        Task<IEnumerable<AppointmentType>> GetAllAsync();
    }
}
