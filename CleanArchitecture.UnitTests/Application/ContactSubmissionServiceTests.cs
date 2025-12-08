using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Email;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Options;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CleanArchitecture.UnitTests.Application
{
    public class ContactSubmissionServiceTests
    {
        private readonly Mock<IContactSubmissionRepository> _repo;
        private readonly Mock<ICountryRepository> _countries;
        private readonly Mock<IEmailQueue> _emailQueue;
        private readonly Mock<ICaptchaValidator> _captcha;
        private readonly Mock<ILogger<ContactSubmissionService>> _logger;

        private readonly ContactSubmissionService _service;

        public ContactSubmissionServiceTests()
        {
            _repo = new Mock<IContactSubmissionRepository>();
            _countries = new Mock<ICountryRepository>();
            _emailQueue = new Mock<IEmailQueue>();
            _captcha = new Mock<ICaptchaValidator>();
            _logger = new Mock<ILogger<ContactSubmissionService>>();

            var smtp = Options.Create(new SmtpFromOptions
            {
                FromEmail = "no-reply@test.com",
                FromName = "Test Team",
                AdminEmail = "admin@test.com"
            });

            _service = new ContactSubmissionService(
                _repo.Object,
                _countries.Object,
                _emailQueue.Object,
                _logger.Object,
                smtp,
                _captcha.Object
            );
        }

        // ----------------------------------------------------------
        // CAPTCHA FAIL
        // ----------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldThrow_WhenCaptchaFails()
        {
            // Arrange
            var dto = new CreateContactSubmissionDto(
                "John", "Doe", "john@test.com", "123456", "+91",
                Guid.NewGuid(), "MP", "Indore", "Subject", "Message",
                CaptchaToken: "badtoken",
                IpAddress: "1.2.3.4"
            );

            _captcha.Setup(x => x.ValidateAsync("badtoken", "1.2.3.4"))
                    .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.CreateAsync(dto));
        }

        // ----------------------------------------------------------
        // CREATE + SAVE TO DATABASE
        // ----------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldSaveContactSubmission()
        {
            // Arrange
            var dto = validDto();
            _captcha.Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

            var saved = new ContactSubmission { Id = Guid.NewGuid(), Email = "john@test.com" };
            _repo.Setup(r => r.AddAsync(It.IsAny<ContactSubmission>()))
                 .ReturnsAsync(saved);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            _repo.Verify(r => r.AddAsync(It.IsAny<ContactSubmission>()), Times.Once);
            Assert.Equal(saved.Id, result.Id);
        }

        // ----------------------------------------------------------
        // USER EMAIL QUEUE TEST
        // ----------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldQueue_UserEmail_IfEmailExists()
        {
            // Arrange
            var dto = validDto();
            _captcha.Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

            var saved = new ContactSubmission { Id = Guid.NewGuid(), Email = "user@test.com" };
            _repo.Setup(r => r.AddAsync(It.IsAny<ContactSubmission>()))
                 .ReturnsAsync(saved);

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _emailQueue.Verify(x => x.EnqueueAsync(It.Is<QueuedEmail>(
                e => e.ToEmail == "user@test.com"
            )), Times.Once);
        }

        // ----------------------------------------------------------
        // ADMIN EMAIL QUEUE TEST
        // ----------------------------------------------------------
        [Fact]
        public async Task CreateAsync_ShouldQueue_AdminEmail()
        {
            // Arrange
            var dto = validDto();
            _captcha.Setup(x => x.ValidateAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(true);

            var saved = new ContactSubmission { Id = Guid.NewGuid(), Email = "user@test.com" };
            _repo.Setup(r => r.AddAsync(It.IsAny<ContactSubmission>()))
                 .ReturnsAsync(saved);

            // Act
            await _service.CreateAsync(dto);

            // Assert
            _emailQueue.Verify(x => x.EnqueueAsync(It.Is<QueuedEmail>(
                e => e.ToEmail == "admin@test.com"
            )), Times.Once);
        }

        // ----------------------------------------------------------
        // GET BY ID
        // ----------------------------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnMappedDto()
        {
            var id = Guid.NewGuid();
            var entity = new ContactSubmission
            {
                Id = id,
                FirstName = "John",
                CountryId = Guid.NewGuid(),
                Email = "john@test.com"
            };

            _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

            _countries.Setup(c => c.GetByIdAsync(entity.CountryId.Value))
                .ReturnsAsync(new Country { Name = "India" });

            var dto = await _service.GetByIdAsync(id);

            Assert.Equal("John", dto?.FirstName);
            Assert.Equal("India", dto?.Country);
        }

        // ----------------------------------------------------------
        // GET ALL
        // ----------------------------------------------------------
        [Fact]
        public async Task GetAllAsync_ShouldReturnList()
        {
            _repo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ContactSubmission> { new ContactSubmission() });

            var result = await _service.GetAllAsync();

            Assert.Single(result);
        }


        // ----------------------------------------------------------
        // Helper
        // ----------------------------------------------------------
        private CreateContactSubmissionDto validDto()
        {
            return new CreateContactSubmissionDto(
                "John", "Doe", "john@test.com", "123456", "+91",
                Guid.NewGuid(), "MP", "Indore", "Subject", "Message",
                CaptchaToken: "valid", IpAddress: "1.2.3.4"
            );
        }
    }
}
