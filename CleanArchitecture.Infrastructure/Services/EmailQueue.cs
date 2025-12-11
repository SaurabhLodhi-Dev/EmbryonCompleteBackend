

//using CleanArchitecture.Application.Email;
//using CleanArchitecture.Application.Interfaces;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace CleanArchitecture.Infrastructure.Services
//{
//    public class EmailQueue : IEmailQueue, IDisposable
//    {
//        private readonly Channel<QueuedEmail> _channel;
//        private readonly ILogger<EmailQueue> _logger;

//        public EmailQueue(ILogger<EmailQueue> logger, int? testCapacity = null)
//        {
//            // Allow override capacity during testing
//            var options = new BoundedChannelOptions(testCapacity ?? 1000)
//            {
//                SingleReader = true,
//                SingleWriter = false,
//                FullMode = BoundedChannelFullMode.Wait
//            };

//            _channel = Channel.CreateBounded<QueuedEmail>(options);
//            _logger = logger;
//        }

//        public async ValueTask EnqueueAsync(QueuedEmail email)
//        {
//            if (email == null)
//                throw new ArgumentNullException(nameof(email));

//            await _channel.Writer.WriteAsync(email);
//            _logger.LogDebug("Enqueued email to {To}. Type={Type}", email.ToEmail, email.Type);
//        }

//        // Required by BackgroundService
//        public ChannelReader<QueuedEmail> Reader => _channel.Reader;

//        // Required by health checks
//        public int Length => _channel.Reader.Count;

//        // Required for tests to finish queued messages
//        public void Complete()
//        {
//            try
//            {
//                _channel.Writer.Complete();
//            }
//            catch { }
//        }

//        // Dispose => also complete the channel
//        public void Dispose()
//        {
//            Complete();
//        }
//    }
//}


using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

public class EmailQueue : IEmailQueue, IDisposable
{
    private readonly Channel<QueuedEmail> _channel;
    private readonly ILogger<EmailQueue> _logger;

    public EmailQueue(ILogger<EmailQueue> logger, int? testCapacity = null)
    {
        var options = new BoundedChannelOptions(testCapacity ?? 1000)
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

    // Expose reader strongly-typed
    public ChannelReader<QueuedEmail> Reader => _channel.Reader;

    // approximate length — safe (not atomic)
    public int Length => _channel.Reader.Count;

    public void Complete()
    {
        try { _channel.Writer.Complete(); } catch { }
    }

    public void Dispose() => Complete();
}
