using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class ErrorLogTests
    {
        [Fact]
        public void Should_Map_Properties_Correctly()
        {
            var id = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var log = new ErrorLog
            {
                Id = id,
                ErrorMessage = "NullReferenceException",
                StackTrace = "stack trace",
                Endpoint = "/api/test",
                HttpMethod = "GET",
                UserAgent = "Mozilla",
                IpAddress = "1.2.3.4",
                CreatedAt = now
            };

            Assert.Equal(id, log.Id);
            Assert.Equal("NullReferenceException", log.ErrorMessage);
            Assert.Equal("stack trace", log.StackTrace);
            Assert.Equal("/api/test", log.Endpoint);
            Assert.Equal("GET", log.HttpMethod);
            Assert.Equal("Mozilla", log.UserAgent);
            Assert.Equal("1.2.3.4", log.IpAddress);
            Assert.Equal(now, log.CreatedAt);
        }
    }
}
