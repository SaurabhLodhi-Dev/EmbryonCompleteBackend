using Microsoft.Extensions.Diagnostics.HealthChecks;
using CleanArchitecture.Application.Interfaces;

public class EmailQueueHealthCheck : IHealthCheck
{
    private readonly IEmailQueue? _queue;

    public EmailQueueHealthCheck(IEmailQueue queue)
    {
        _queue = queue;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_queue == null)
            return Task.FromResult(HealthCheckResult.Unhealthy("EmailQueue is null"));

        int length = _queue.Length;

        if (length > 200)
            return Task.FromResult(HealthCheckResult.Unhealthy($"Queue stuck ({length} emails)"));

        if (length > 50)
            return Task.FromResult(HealthCheckResult.Degraded($"Queue backlog ({length})"));

        return Task.FromResult(HealthCheckResult.Healthy("Queue OK"));
    }
}
