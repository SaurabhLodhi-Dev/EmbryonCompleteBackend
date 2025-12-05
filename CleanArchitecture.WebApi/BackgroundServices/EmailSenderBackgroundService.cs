using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.WebApi.BackgroundServices
{
    public class EmailSenderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailSenderBackgroundService> _logger;
        private readonly IEmailQueue _queue;
        private readonly int _maxRetries = 3;
        private readonly TimeSpan _initialBackoff = TimeSpan.FromSeconds(5);

        public EmailSenderBackgroundService(IServiceProvider serviceProvider, IEmailQueue queue, ILogger<EmailSenderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email background service started.");

            // EmailQueue implementation exposes ChannelReader via EmailQueue.Reader
            if (_queue is null)
            {
                _logger.LogError("Email queue is null.");
                return;
            }

            // We must create scope per message to use scoped services (DbContext etc.)
            var readerProperty = _queue.GetType().GetProperty("Reader");
            if (readerProperty == null)
            {
                _logger.LogError("Email queue does not expose Reader property.");
                return;
            }

            var reader = (System.Threading.Channels.ChannelReader<QueuedEmail>)readerProperty.GetValue(_queue)!;

            await foreach (var queued in reader.ReadAllAsync(stoppingToken))
            {
                if (queued == null) continue;
                await ProcessEmailAsync(queued, stoppingToken);
            }
        }

        private async Task ProcessEmailAsync(QueuedEmail queued, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailSenderBackgroundService>>();

            // create notification record (initially pending)
            var notification = new Notification
            {
                Type = queued.Type,
                ToAddress = queued.ToEmail,
                Subject = queued.Subject,
                Message = queued.HtmlBody,
                Status = "pending",
                SentAt = null,
            };

            try
            {
                // Persist pending notification
                var created = await repo.AddAsync(notification);

                // send with retries
                int attempt = 0;
                TimeSpan backoff = _initialBackoff;
                Exception? lastEx = null;

                while (attempt < _maxRetries && !cancellationToken.IsCancellationRequested)
                {
                    attempt++;
                    queued.Attempt = attempt;
                    try
                    {
                        await emailSender.SendAsync(queued.FromName, queued.FromEmail, queued.ToEmail, queued.Subject, queued.HtmlBody, queued.PlainBody);

                        // Update notification as sent
                        created.Status = "sent";
                        created.ErrorMessage = null;
                        created.SentAt = DateTime.UtcNow;
                        await repo.UpdateAsync(created);

                        logger.LogInformation("Email sent to {To} (type={Type})", queued.ToEmail, queued.Type);
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                        logger.LogWarning(ex, "Failed to send email to {To} attempt {Attempt}. Backoff {Backoff}s", queued.ToEmail, attempt, backoff.TotalSeconds);

                        // update notification with latest failure
                        created.Status = "retrying";
                        created.ErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
                        created.UpdatedAt = DateTime.UtcNow;
                        await repo.UpdateAsync(created);

                        // exponential backoff
                        await Task.Delay(backoff, cancellationToken);
                        backoff = backoff * 2;
                    }
                }

                // If we reach here, all retries failed
                notification.Status = "failed";
                notification.ErrorMessage = lastEx?.ToString() ?? "Unknown error";
                notification.SentAt = DateTime.UtcNow;
                await repo.UpdateAsync(notification);

                logger.LogError(lastEx, "Failed to deliver email to {To} after {Attempts} attempts", queued.ToEmail, _maxRetries);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception while processing queued email to {To}", queued.ToEmail);
                // Try to persist final failure if repository available
                try
                {
                    if (repo != null)
                    {
                        var failed = new Notification
                        {
                            Type = queued.Type,
                            ToAddress = queued.ToEmail,
                            Subject = queued.Subject,
                            Message = queued.HtmlBody,
                            Status = "failed",
                            ErrorMessage = ex.ToString(),
                            SentAt = DateTime.UtcNow
                        };
                        await repo.AddAsync(failed);
                    }
                }
                catch (Exception inner)
                {
                    logger.LogError(inner, "Failed to persist failed notification for {To}", queued.ToEmail);
                }
            }
        }
    }
}
