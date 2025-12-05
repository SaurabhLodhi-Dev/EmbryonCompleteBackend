#nullable enable
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly ApplicationDbContext _context;

        public CountryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await _context.Countries
                                 .AsNoTracking()
                                 .OrderBy(x => x.Name)
                                 .ToListAsync();
        }

        public async Task<Country?> GetByIdAsync(Guid id)
        {
            return await _context.Countries
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
