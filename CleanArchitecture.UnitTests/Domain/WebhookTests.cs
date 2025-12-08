using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class WebhookTests
    {
        [Fact]
        public void Should_Set_Properties()
        {
            var id = Guid.NewGuid();

            var w = new Webhook
            {
                Id = id,
                Url = "https://webhook.com",
                Event = "contact.created",
                SecretKey = "secret-123"
            };

            Assert.Equal(id, w.Id);
            Assert.Equal("https://webhook.com", w.Url);
            Assert.Equal("contact.created", w.Event);
            Assert.Equal("secret-123", w.SecretKey);
        }
    }
}
