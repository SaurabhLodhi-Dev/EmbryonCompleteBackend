using CleanArchitecture.Application.Email;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IEmailQueue
    {
        ValueTask EnqueueAsync(QueuedEmail email);

        ChannelReader<QueuedEmail> Reader { get; }   // Required for background service

        int Length { get; }                          // For monitoring + health checks

        void Complete();                             // Required for tests to close channel
    }
}
