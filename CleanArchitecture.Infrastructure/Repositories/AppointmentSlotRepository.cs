#nullable enable
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class AppointmentSlotRepository : IAppointmentSlotRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentSlotRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentSlot>> GetAllAsync()
        {
            return await _context.AppointmentSlots
                                 .AsNoTracking()
                                 .OrderBy(x => x.SlotDate)
                                 .ThenBy(x => x.StartTime)
                                 .ToListAsync();
        }

        public async Task<AppointmentSlot> GetByIdAsync(Guid id)
        {
            var slot = await _context.AppointmentSlots
     .AsNoTracking()
     .FirstOrDefaultAsync(x => x.Id == id);


            if (slot == null)
                throw new KeyNotFoundException($"AppointmentSlot with id {id} not found.");

            return slot;
        }

        public async Task MarkSlotAsBookedAsync(Guid id)
        {
            var slot = await _context.AppointmentSlots.FirstOrDefaultAsync(x => x.Id == id);

            if (slot == null)
                throw new KeyNotFoundException($"AppointmentSlot with id {id} not found.");

            // If already booked, no-op or throw depending on your policy
            if (slot.IsBooked == true)
                throw new InvalidOperationException("The appointment slot is already booked.");

            slot.IsBooked = true;
            slot.UpdatedAt = DateTime.UtcNow;

            _context.AppointmentSlots.Update(slot);
            await _context.SaveChangesAsync();
        }
    }
}
