using System;
using CleanArchitecture.Domain.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests.Domain
{
    public class ContactSubmissionTests
    {
        [Fact]
        public void Can_Create_ContactSubmission_And_Set_Properties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            // Act
            var contact = new ContactSubmission
            {
                Id = id,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                PhoneCountryCode = "+91",
                State = "MP",
                City = "Indore",
                Subject = "Test Subject",
                Message = "Hello there",
                IpAddress = "1.2.3.4",
                UserAgent = "Mozilla",
                CreatedAt = createdAt
            };

            // Assert
            Assert.Equal(id, contact.Id);
            Assert.Equal("John", contact.FirstName);
            Assert.Equal("Doe", contact.LastName);
            Assert.Equal("john@example.com", contact.Email);
            Assert.Equal("1234567890", contact.Phone);
            Assert.Equal("+91", contact.PhoneCountryCode);
            Assert.Equal("MP", contact.State);
            Assert.Equal("Indore", contact.City);
            Assert.Equal("Test Subject", contact.Subject);
            Assert.Equal("Hello there", contact.Message);
            Assert.Equal("1.2.3.4", contact.IpAddress);
            Assert.Equal("Mozilla", contact.UserAgent);
            Assert.Equal(createdAt, contact.CreatedAt);
        }
    }
}
