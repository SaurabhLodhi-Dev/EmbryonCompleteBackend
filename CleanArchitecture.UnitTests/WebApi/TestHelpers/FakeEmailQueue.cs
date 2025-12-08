using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using System.Threading.Channels;
using System.Threading.Tasks;

public class FakeEmailQueue : IEmailQueue
{
    private readonly Channel<QueuedEmail> _channel;

    public FakeEmailQueue(int capacity = 10)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };

        _channel = Channel.CreateBounded<QueuedEmail>(options);
    }

    public ValueTask EnqueueAsync(QueuedEmail email)
    {
        return _channel.Writer.WriteAsync(email);
    }

    // Required by IEmailQueue
    public ChannelReader<QueuedEmail> Reader => _channel.Reader;

    // Required by EmailQueueHealthCheck + tests
    public int Length => _channel.Reader.Count;

    // Allows tests to notify the BackgroundService that no more emails will come
    public void Complete()
    {
        try { _channel.Writer.Complete(); } catch { }
    }
}
