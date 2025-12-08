using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.WebApi.BackgroundServices;
using CleanArchitecture.UnitTests.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.UnitTests.BackgroundServices
{
    public class EmailSenderBackgroundServiceTests
    {
        private ServiceProvider BuildServiceProvider(
            IEmailSender emailSender,
            INotificationRepository notificationRepo
        )
        {
            var services = new ServiceCollection();

            services.AddSingleton(emailSender);
            services.AddSingleton(notificationRepo);

            services.AddSingleton<ILogger<EmailSenderBackgroundService>>(
                new Mock<ILogger<EmailSenderBackgroundService>>().Object);

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Should_Send_Email_Successfully()
        {
            // Arrange
            var queue = new FakeEmailQueue();

            var senderMock = new Mock<IEmailSender>();
            senderMock.Setup(s => s.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(Task.CompletedTask); // SUCCESS

            var notificationRepo = new Mock<INotificationRepository>();
            notificationRepo.Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => n);
            notificationRepo.Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            var provider = BuildServiceProvider(senderMock.Object, notificationRepo.Object);

            var worker = new EmailSenderBackgroundService(provider, queue,
                new Mock<ILogger<EmailSenderBackgroundService>>().Object);

            var email = new QueuedEmail
            {
                ToEmail = "user@test.com",
                FromEmail = "noreply@test.com",
                FromName = "System",
                Subject = "Test Email",
                HtmlBody = "<p>Hello</p>",
                PlainBody = "Hello",
                Type = "test"
            };

            await queue.EnqueueAsync(email);
            //queue.Complete();

            // Act
            await worker.StartAsync(CancellationToken.None);
            await worker.StopAsync(CancellationToken.None);

            // Assert
            senderMock.Verify(s => s.SendAsync(
                "System", "noreply@test.com", "user@test.com",
                "Test Email", "<p>Hello</p>", "Hello"
            ), Times.Once);

            notificationRepo.Verify(r => r.UpdateAsync(It.Is<Notification>(n =>
                n.Status == "sent"
            )), Times.Once);
        }

        [Fact]
        public async Task Should_Retry_And_Fail_When_Email_Cannot_Send()
        {
            // Arrange
            var queue = new FakeEmailQueue();

            var senderMock = new Mock<IEmailSender>();
            senderMock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()
            )).ThrowsAsync(new Exception("SMTP error"));

            var notificationRepo = new Mock<INotificationRepository>();
            notificationRepo.Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => n);
            notificationRepo.Setup(r => r.UpdateAsync(It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            var provider = BuildServiceProvider(senderMock.Object, notificationRepo.Object);

            var worker = new EmailSenderBackgroundService(provider, queue,
                new Mock<ILogger<EmailSenderBackgroundService>>().Object);

            await queue.EnqueueAsync(new QueuedEmail
            {
                ToEmail = "x@test.com",
                FromEmail = "no@test.com",
                FromName = "Sys",
                Subject = "FailTest",
                HtmlBody = "X",
                PlainBody = "X",
                Type = "test"
            });

            //queue.Complete();

            // Act
            await worker.StartAsync(CancellationToken.None);
            await worker.StopAsync(CancellationToken.None);

            // Assert
            senderMock.Verify(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()
            ), Times.AtLeast(3)); // because of retry logic

            notificationRepo.Verify(r => r.UpdateAsync(It.Is<Notification>(n =>
                n.Status == "failed"
            )), Times.Once);
        }
    }
}
