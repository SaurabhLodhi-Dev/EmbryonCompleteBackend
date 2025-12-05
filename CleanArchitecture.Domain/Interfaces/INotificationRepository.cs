using CleanArchitecture.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> AddAsync(Notification entity);
        Task UpdateAsync(Notification entity);
    }
}
