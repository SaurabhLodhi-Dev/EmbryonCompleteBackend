#nullable enable
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> AddAsync(Appointment entity)
        {
            await _context.Appointments.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _context.Appointments
      .Include(a => a.AppointmentType)
      .Include(a => a.Slot)
      .AsNoTracking()
      .OrderByDescending(x => x.CreatedAt)
      .ToListAsync();

        }

        public async Task<Appointment> GetByIdAsync(Guid id)
        {
            var entity = await _context.Appointments
                    .Include(a => a.AppointmentType)
                    .Include(a => a.Slot)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"Appointment with id {id} not found.");

            return entity;
        }

    }
}
