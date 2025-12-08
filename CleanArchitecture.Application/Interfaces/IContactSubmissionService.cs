using CleanArchitecture.Application.DTOs.Contact;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IContactSubmissionService
    {
        Task<ContactSubmissionDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<ContactSubmissionDto>> GetAllAsync();
        Task<ContactSubmissionDto> CreateAsync(CreateContactSubmissionDto dto);
    }
}
