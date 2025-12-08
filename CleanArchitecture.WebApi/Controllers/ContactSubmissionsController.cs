//using CleanArchitecture.Application.DTOs.Contact;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.WebApi.Middlewares;
//using Microsoft.AspNetCore.Mvc;

//namespace CleanArchitecture.WebApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ContactSubmissionsController : ControllerBase
//    {
//        private readonly IContactSubmissionService _service;


//        public ContactSubmissionsController(
//            IContactSubmissionService service)
//        {
//            _service = service;

//        }

//        // ----------------------------------------------------------------------
//        // GET ALL
//        // ----------------------------------------------------------------------
//        [HttpGet]
//        [ProducesResponseType(typeof(IEnumerable<ContactSubmissionDto>), StatusCodes.Status200OK)]
//        public async Task<ActionResult<IEnumerable<ContactSubmissionDto>>> GetAll()
//        {
//            var list = await _service.GetAllAsync();
//            return Ok(list);
//        }

//        // ----------------------------------------------------------------------
//        // GET BY ID
//        // ----------------------------------------------------------------------
//        [HttpGet("{id:guid}")]
//        [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public async Task<ActionResult<ContactSubmissionDto>> GetById(Guid id)
//        {
//            if (id == Guid.Empty)
//                return BadRequest("Invalid ID.");

//            var result = await _service.GetByIdAsync(id);

//            if (result == null)
//                return NotFound($"Contact submission with ID {id} not found.");

//            return Ok(result);
//        }

//        // ----------------------------------------------------------------------
//        // CREATE
//        // ----------------------------------------------------------------------
//        [HttpPost]
//        [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status201Created)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        public async Task<ActionResult<ContactSubmissionDto>> Create([FromBody] CreateContactSubmissionDto dto)
//        {
//            var geo = HttpContext.Items["GeoInfo"] as GeoInfo;

//            dto = dto with
//            {
//                IpAddress = geo?.ip,
//                City = string.IsNullOrWhiteSpace(dto.City) ? geo?.city : dto.City,
//                State = string.IsNullOrWhiteSpace(dto.State) ? geo?.region : dto.State
//            };

//            var created = await _service.CreateAsync(dto);

//            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
//        }

//        // ======================================================================
//        // NEW ENDPOINT #1: TEST EMAIL
//        // ======================================================================
//        /// <summary>
//        /// Sends a test email to verify SMTP configuration.
//        /// </summary>


//        // ======================================================================
//        // NEW ENDPOINT #2: RESEND CONFIRMATION EMAIL
//        // ======================================================================
//        /// <summary>
//        /// Resends the confirmation email for a specific contact submission.
//        /// </summary>

//    }
//}


using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.WebApi.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactSubmissionsController : ControllerBase
    {
        private readonly IContactSubmissionService _service;
        private readonly ILogger<ContactSubmissionsController> _logger;

        public ContactSubmissionsController(
            IContactSubmissionService service,
            ILogger<ContactSubmissionsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // ----------------------------------------------------------------------
        // GET ALL
        // ----------------------------------------------------------------------
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ContactSubmissionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContactSubmissionDto>>> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // ----------------------------------------------------------------------
        // GET BY ID
        // ----------------------------------------------------------------------
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

        // ----------------------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------------------
        [HttpPost]
        [ProducesResponseType(typeof(ContactSubmissionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContactSubmissionDto>> Create([FromBody] CreateContactSubmissionDto dto)
        {
            // ✅ Get GeoInfo from middleware
            var geo = HttpContext.Items["GeoInfo"] as GeoInfo;

            // Optional: log GeoInfo for debugging
            _logger.LogInformation("GeoInfo: {@Geo}", geo);

            // ✅ Sanitize and fill missing values
            var sanitizedDto = dto with
            {
                FirstName = Sanitizer.Clean(dto.FirstName),
                LastName = Sanitizer.Clean(dto.LastName),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                PhoneCountryCode = dto.PhoneCountryCode?.Trim(),
                CountryId = dto.CountryId,
                State = string.IsNullOrWhiteSpace(dto.State) ? Sanitizer.Clean(geo?.region) : Sanitizer.Clean(dto.State),
                City = string.IsNullOrWhiteSpace(dto.City) ? Sanitizer.Clean(geo?.city) : Sanitizer.Clean(dto.City),
                Subject = Sanitizer.Clean(dto.Subject),
                Message = Sanitizer.Clean(dto.Message),
                IpAddress = geo?.ip
            };

            var created = await _service.CreateAsync(sanitizedDto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ======================================================================
        // NEW ENDPOINT #1: TEST EMAIL
        // ======================================================================
        /// <summary>
        /// Sends a test email to verify SMTP configuration.
        /// </summary>
        [HttpPost("test-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult SendTestEmail()
        {
            // TODO: implement sending a test email using your email service
            return Ok(new { success = true, message = "Test email sent (stub)." });
        }

        // ======================================================================
        // NEW ENDPOINT #2: RESEND CONFIRMATION EMAIL
        // ======================================================================
        /// <summary>
        /// Resends the confirmation email for a specific contact submission.
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
                return NotFound($"Contact submission with ID {id} not found.");

            // TODO: implement resending confirmation email using your email service

            return Ok(new { success = true, message = "Confirmation email resent (stub)." });
        }
    }

    // -------------------------------------------------------------------------
    // Simple Sanitizer Helper
    // -------------------------------------------------------------------------
    public static class Sanitizer
    {
        public static string? Clean(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            var trimmed = input.Trim();
            // Remove control characters
            return new string(trimmed.Where(c => !char.IsControl(c)).ToArray());
        }
    }
}
