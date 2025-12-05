using CleanArchitecture.Application.Email;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IEmailQueue
    {
        ValueTask EnqueueAsync(QueuedEmail email);
    }
}
