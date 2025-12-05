#nullable enable
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class AppointmentTypeRepository : IAppointmentTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentType>> GetAllAsync()
        {
            return await _context.AppointmentTypes
                                 .AsNoTracking()
                                 .OrderBy(x => x.Name)
                                 .ToListAsync();
        }

        public async Task<AppointmentType?> GetByIdAsync(Guid id)
        {
            return await _context.AppointmentTypes
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
