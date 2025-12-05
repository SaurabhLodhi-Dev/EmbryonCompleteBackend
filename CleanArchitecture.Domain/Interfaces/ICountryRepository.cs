using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface ICountryRepository
    {
        Task<Country?> GetByIdAsync(Guid id);
        Task<IEnumerable<Country>> GetAllAsync();
    }
}
