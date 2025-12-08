using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.UnitTests.Infrastructure
{
    public class EmailQueueTests
    {
        private EmailQueue CreateQueue()
        {
            return new EmailQueue(new Mock<ILogger<EmailQueue>>().Object);
        }

        [Fact]
        public async Task EnqueueAsync_ShouldWriteMessageToChannel()
        {
            // Arrange
            var queue = CreateQueue();
            var reader = queue.Reader;

            var email = new QueuedEmail
            {
                ToEmail = "test@example.com",
                Subject = "Hello",
                HtmlBody = "<p>Test</p>"
            };

            // Act
            await queue.EnqueueAsync(email);

            // Assert
            var result = await reader.ReadAsync();
            Assert.Equal("test@example.com", result.ToEmail);
            Assert.Equal("Hello", result.Subject);
        }

        [Fact]
        public async Task Queue_ShouldMaintain_FIFO_Order()
        {
            // Arrange
            var queue = CreateQueue();
            var reader = queue.Reader;

            var emails = new List<QueuedEmail>
            {
                new QueuedEmail { Subject = "First" },
                new QueuedEmail { Subject = "Second" },
                new QueuedEmail { Subject = "Third" }
            };

            foreach (var e in emails)
                await queue.EnqueueAsync(e);

            // Act
            var first = await reader.ReadAsync();
            var second = await reader.ReadAsync();
            var third = await reader.ReadAsync();

            // Assert FIFO order
            Assert.Equal("First", first.Subject);
            Assert.Equal("Second", second.Subject);
            Assert.Equal("Third", third.Subject);
        }

        [Fact]
        public async Task Dispose_ShouldCompleteWriter_AndStopQueue()
        {
            // Arrange
            var queue = CreateQueue();
            var reader = queue.Reader;

            // Act
            queue.Dispose();

            // Assert: Reading after dispose should hit Completed state
            Assert.True(reader.Completion.IsCompleted);
        }

        [Fact]
        public async Task Enqueue_FromMultipleTasks_ShouldRemainThreadSafe()
        {
            // Arrange
            var queue = CreateQueue();
            var reader = queue.Reader;

            int count = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < count; i++)
            {
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    await queue.EnqueueAsync(new QueuedEmail
                    {
                        Subject = $"Email-{index}"
                    });
                }));
            }

            await Task.WhenAll(tasks);

            // Assert: Read 100 items without corruption
            var subjects = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var email = await reader.ReadAsync();
                subjects.Add(email.Subject);
            }

            Assert.Equal(100, subjects.Count);
            Assert.Contains("Email-0", subjects);
            Assert.Contains("Email-99", subjects);
        }

        [Fact]
        public async Task Enqueue_ShouldWait_WhenChannelIsFull()
        {
            // ARRANGE: Create a small bounded queue of size 1
            var logger = new Mock<ILogger<EmailQueue>>();
            var queue = new EmailQueue(logger.Object);

            var reader = queue.Reader;

            // Write first message (fills channel)
            await queue.EnqueueAsync(new QueuedEmail { Subject = "First" });

            var secondEmail = new QueuedEmail { Subject = "Second" };

            var enqueueTask = Task.Run(async () =>
            {
                await queue.EnqueueAsync(secondEmail); // will wait
            });

            // Wait a short moment to ensure enqueue is blocked
            await Task.Delay(50);

            Assert.False(enqueueTask.IsCompleted);

            // Now read the first item → frees space → second enqueue completes
            _ = await reader.ReadAsync();

            await enqueueTask; // now should complete

            var secondRead = await reader.ReadAsync();
            Assert.Equal("Second", secondRead.Subject);
        }
    }
}
