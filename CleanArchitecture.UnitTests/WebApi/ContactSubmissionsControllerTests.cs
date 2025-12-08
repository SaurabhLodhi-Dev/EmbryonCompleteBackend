using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.WebApi.Controllers;
using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.UnitTests.WebApi
{
    public class ContactSubmissionsControllerTests
    {
        private readonly Mock<IContactSubmissionService> _service;
        private readonly Mock<ILogger<ContactSubmissionsController>> _logger;

        public ContactSubmissionsControllerTests()
        {
            _service = new Mock<IContactSubmissionService>();
            _logger = new Mock<ILogger<ContactSubmissionsController>>();
        }

        private ContactSubmissionsController CreateController()
        {
            var controller = new ContactSubmissionsController(_service.Object, _logger.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return controller;
        }

        // ---------------------------------------------------------
        // GET ALL
        // ---------------------------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            var controller = CreateController();

            var list = new List<ContactSubmissionDto>
            {
                new ContactSubmissionDto(
                    Guid.NewGuid(), "John", "Doe", "john@test.com",
                    "123", "+91", "India", "MP", "Indore",
                    "Hello", "Msg", "1.2.3.4", DateTime.UtcNow
                )
            };

            _service.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<ContactSubmissionDto>>(ok.Value);
            Assert.Single(data);
        }

        // ---------------------------------------------------------
        // GET BY ID: INVALID GUID
        // ---------------------------------------------------------
        [Fact]
        public async Task GetById_WithEmptyGuid_ShouldReturnBadRequest()
        {
            var controller = CreateController();

            var result = await controller.GetById(Guid.Empty);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ---------------------------------------------------------
        // GET BY ID: NOT FOUND
        // ---------------------------------------------------------
        [Fact]
        public async Task GetById_WhenNotFound_ShouldReturnNotFound()
        {
            var controller = CreateController();
            var id = Guid.NewGuid();

            _service.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync((ContactSubmissionDto?)null);

            var result = await controller.GetById(id);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ---------------------------------------------------------
        // GET BY ID: FOUND
        // ---------------------------------------------------------
        [Fact]
        public async Task GetById_WhenExists_ShouldReturnOk()
        {
            var controller = CreateController();
            var id = Guid.NewGuid();

            var dto = new ContactSubmissionDto(
                id, "John", "Doe", "john@test.com",
                "123", "+91", "India", "MP", "Indore",
                "Hello", "Msg", "1.2.3.4", DateTime.UtcNow
            );

            _service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

            var result = await controller.GetById(id);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<ContactSubmissionDto>(ok.Value);
            Assert.Equal(id, data.Id);
        }

        // ---------------------------------------------------------
        // CREATE: Should Use GeoInfo from HttpContext
        // ---------------------------------------------------------
        [Fact]
        public async Task Create_ShouldInjectGeoInfo_FromMiddleware()
        {
            var controller = CreateController();

            // Simulate Geo middleware
            var geo = new GeoInfo
            {
                ip = "5.6.7.8",
                city = "Mumbai",
                region = "MH"
            };
            controller.HttpContext.Items["GeoInfo"] = geo;

            var dto = new CreateContactSubmissionDto(
                "John", "Doe", "john@test.com", "99999", "+91",
                Guid.NewGuid(), null, null, "Sub", "Msg",
                "token123", "IGNORED" // Will be replaced by GeoInfo
            );

            var returned = new ContactSubmissionDto(
                Guid.NewGuid(), "John", "Doe", "john@test.com",
                "99999", "+91", "India", "MH", "Mumbai",
                "Sub", "Msg", "5.6.7.8", DateTime.UtcNow
            );

            _service.Setup(s => s.CreateAsync(It.IsAny<CreateContactSubmissionDto>()))
                    .ReturnsAsync(returned);

            // Act
            var result = await controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ContactSubmissionDto>(created.Value);

            Assert.Equal("5.6.7.8", response.IpAddress);
            Assert.Equal("MH", response.State);
            Assert.Equal("Mumbai", response.City);
        }

        // ---------------------------------------------------------
        // CREATE: Sanitizer should clean input
        // ---------------------------------------------------------
        [Fact]
        public async Task Create_ShouldSanitize_UserInput()
        {
            var controller = CreateController();
            controller.HttpContext.Items["GeoInfo"] = new GeoInfo { ip = "9.9.9.9" };

            var dto = new CreateContactSubmissionDto(
                " John ", " Doe ", "john@test.com ", " 12345 ", "+91 ",
                Guid.NewGuid(), " MP ", " Indore ", " Sub ", " Msg ",
                "token123", "9.9.9.9"
            );

            var returned = new ContactSubmissionDto(
                Guid.NewGuid(), "John", "Doe", "john@test.com",
                "12345", "+91", "India", "MP", "Indore",
                "Sub", "Msg", "9.9.9.9", DateTime.UtcNow
            );

            _service.Setup(s => s.CreateAsync(It.IsAny<CreateContactSubmissionDto>()))
                .ReturnsAsync(returned);

            var result = await controller.Create(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var data = Assert.IsType<ContactSubmissionDto>(created.Value);

            Assert.Equal("John", data.FirstName);
            Assert.Equal("Doe", data.LastName);
        }
    }
}
