using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace CleanArchitecture.Infrastructure.Services
{
    public class EmailQueue : IEmailQueue, IDisposable
    {
        private readonly Channel<QueuedEmail> _channel;
        private readonly ILogger<EmailQueue> _logger;

        public EmailQueue(ILogger<EmailQueue> logger)
        {
            // Bounded channel to avoid unbounded memory growth. Adjust capacity as needed.
            var options = new BoundedChannelOptions(1000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };

            _channel = Channel.CreateBounded<QueuedEmail>(options);
            _logger = logger;
        }

        public async ValueTask EnqueueAsync(QueuedEmail email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            await _channel.Writer.WriteAsync(email);
            _logger.LogDebug("Enqueued email to {To}. Type={Type}", email.ToEmail, email.Type);
        }

        public ChannelReader<QueuedEmail> Reader => _channel.Reader;

        public void Dispose()
        {
            try
            {
                _channel.Writer.Complete();
            }
            catch { }
        }
    }
}
