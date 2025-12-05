#nullable enable
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ContactSubmissionRepository : IContactSubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactSubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContactSubmission> AddAsync(ContactSubmission entity)
        {
            await _context.ContactSubmissions.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<ContactSubmission>> GetAllAsync()
        {
            return await _context.ContactSubmissions
                                 .AsNoTracking()
                                 .OrderByDescending(x => x.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<ContactSubmission> GetByIdAsync(Guid id)
        {
            var entity = await _context.ContactSubmissions
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new KeyNotFoundException($"ContactSubmission with id {id} not found.");

            return entity;
        }
    }
}
