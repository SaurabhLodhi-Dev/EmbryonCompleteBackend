using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface IAppointmentSlotRepository
    {
        Task<AppointmentSlot> GetByIdAsync(Guid id);
        Task<IEnumerable<AppointmentSlot>> GetAllAsync();
        Task MarkSlotAsBookedAsync(Guid id);
    }
}
