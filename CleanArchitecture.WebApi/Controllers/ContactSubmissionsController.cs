using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    /// <summary>
    /// Manages Contact Form submissions including:
    /// - Creating new submissions
    /// - Fetching all submissions
    /// - Fetching a submission by ID
    /// 
    /// This controller also integrates:
    /// - GeoLocation middleware (auto-fills IP, city, state, country, latitude, longitude)
    /// - Sanitization to prevent harmful input
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContactSubmissionsController : ControllerBase
    {
        private readonly IContactSubmissionService _service;
        private readonly ILogger<ContactSubmissionsController> _logger;

        /// <summary>
        /// Constructor injecting service layer and logger.
        /// </summary>
        public ContactSubmissionsController(
            IContactSubmissionService service,
            ILogger<ContactSubmissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // =====================================================================================
        // GET ALL
        // =====================================================================================

        /// <summary>
        /// Retrieves all contact submissions stored in the system.
        /// </summary>
        /// <returns>List of contact submission DTOs.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContactSubmissionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContactSubmissionDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // =====================================================================================
        // GET BY ID
        // =====================================================================================

        /// <summary>
        /// Fetch a specific contact submission by its unique ID.
        /// </summary>
        /// <param name="id">Submission ID (Guid)</param>
        /// <returns>ContactSubmissionDto if found</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContactSubmissionDto>> GetById(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid ID.");

            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound($"Contact submission with ID {id} not found.");

            return Ok(result);
        }

        // =====================================================================================
        // CREATE (POST)
        // =====================================================================================

        /// <summary>
        /// Creates a new contact form submission.
        /// GeoLocation middlewares automatically provide:
        /// - IP Address
        /// - User Agent
        /// - City, State, Country
        /// - Latitude, Longitude
        /// 
        /// Sanitization is applied to protect against unsafe input.
        /// </summary>
        /// <param name="dto">Incoming request payload from UI</param>
        /// <returns>Created submission DTO</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContactSubmissionDto>> Create([FromBody] CreateContactSubmissionDto dto)
        {
            // Retrieve geolocation information from the middleware
            var geo = HttpContext.Items["GeoInfo"] as GeoInfo;

            // Logging geolocation data for debugging and monitoring
            _logger.LogInformation(
                "GeoInfo Received → IP: {IP}, City: {City}, Region: {Region}, Country: {Country}, Latitude: {Lat}, Longitude: {Lon}",
                geo?.ip, geo?.city, geo?.region, geo?.country_name, geo?.latitude, geo?.longitude
            );

            // Sanitize incoming request + auto-fill missing geolocation properties
            var sanitizedDto = dto with
            {
                FirstName = Sanitizer.Clean(dto.FirstName),
                LastName = Sanitizer.Clean(dto.LastName),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                PhoneCountryCode = dto.PhoneCountryCode?.Trim(),

                // Auto-fill country name if user didn't provide one
                CountryName = string.IsNullOrWhiteSpace(dto.CountryName)
                    ? Sanitizer.Clean(geo?.country_name)
                    : Sanitizer.Clean(dto.CountryName),

                // Auto-fill state from GeoIP
                State = string.IsNullOrWhiteSpace(dto.State)
                    ? Sanitizer.Clean(geo?.region)
                    : Sanitizer.Clean(dto.State),

                // Auto-fill city from GeoIP
                City = string.IsNullOrWhiteSpace(dto.City)
                    ? Sanitizer.Clean(geo?.city)
                    : Sanitizer.Clean(dto.City),

                // Sanitize remaining fields
                Subject = Sanitizer.Clean(dto.Subject),
                Message = Sanitizer.Clean(dto.Message),

                // Assign tracking fields (IP + UA)
                IpAddress = geo?.ip,
                UserAgent = geo?.user_agent,

                // Assign latitude and longitude
                Latitude = geo?.latitude,
                Longitude = geo?.longitude
            };

            // Create and save record via service layer
            var created = await _service.CreateAsync(sanitizedDto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // =====================================================================================
        // TEST EMAIL ENDPOINT (Optional)
        // =====================================================================================

        /// <summary>
        /// Sends a test email to verify SMTP email delivery configuration.
        /// </summary>
        [HttpPost("test-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SendTestEmail()
        {
            // You can implement actual email sending here
            return Ok(new { success = true, message = "Test email sent (stub)." });
        }

        // =====================================================================================
        // RESEND CONFIRMATION EMAIL (Optional)
        // =====================================================================================

        /// <summary>
        /// Resends a confirmation email for a previously created contact submission.
        /// </summary>
        [HttpPost("{id:guid}/resend-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendConfirmationEmail(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest("Invalid ID.");

            var submission = await _service.GetByIdAsync(id);

            if (submission == null)
                return NotFound($"Submission with ID {id} not found.");

            // Implement actual resend logic here
            return Ok(new { success = true, message = "Confirmation email resent (stub)." });
        }
    }

    // =============================================================================================
    // Sanitizer Helper
    // =============================================================================================

    /// <summary>
    /// Sanitizes incoming string fields by trimming whitespace
    /// and removing unsafe control characters.
    /// </summary>
    public static class Sanitizer
    {
        public static string? Clean(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var trimmed = input.Trim();

            // Remove ASCII control characters (security protection)
            return new string(trimmed.Where(c => !char.IsControl(c)).ToArray());
        }
    }
}
