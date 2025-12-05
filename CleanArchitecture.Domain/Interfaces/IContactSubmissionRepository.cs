using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface IContactSubmissionRepository
    {
        Task<ContactSubmission> GetByIdAsync(Guid id);
        Task<IEnumerable<ContactSubmission>> GetAllAsync();
        Task<ContactSubmission> AddAsync(ContactSubmission entity);
    }
}
