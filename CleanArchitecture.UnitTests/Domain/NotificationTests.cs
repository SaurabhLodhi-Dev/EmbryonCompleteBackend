using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class NotificationTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();
            var sentAt = DateTime.UtcNow;

            var n = new Notification
            {
                Id = id,
                Type = "email",
                ToAddress = "test@example.com",
                Subject = "Hello",
                Message = "Body",
                Status = "sent",
                ErrorMessage = null,
                SentAt = sentAt,
                AttemptCount = 2
            };

            Assert.Equal(id, n.Id);
            Assert.Equal("email", n.Type);
            Assert.Equal("test@example.com", n.ToAddress);
            Assert.Equal("Hello", n.Subject);
            Assert.Equal("Body", n.Message);
            Assert.Equal("sent", n.Status);
            Assert.Null(n.ErrorMessage);
            Assert.Equal(sentAt, n.SentAt);
            Assert.Equal(2, n.AttemptCount);
        }
    }
}
