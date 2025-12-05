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
       

        public ContactSubmissionsController(
            IContactSubmissionService service)
        {
            _service = service;
           
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
            var geo = HttpContext.Items["GeoInfo"] as GeoInfo;

            dto = dto with
            {
                IpAddress = geo?.ip,
                City = string.IsNullOrWhiteSpace(dto.City) ? geo?.city : dto.City,
                State = string.IsNullOrWhiteSpace(dto.State) ? geo?.region : dto.State
            };

            var created = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ======================================================================
        // NEW ENDPOINT #1: TEST EMAIL
        // ======================================================================
        /// <summary>
        /// Sends a test email to verify SMTP configuration.
        /// </summary>
       

        // ======================================================================
        // NEW ENDPOINT #2: RESEND CONFIRMATION EMAIL
        // ======================================================================
        /// <summary>
        /// Resends the confirmation email for a specific contact submission.
        /// </summary>
       
    }
}
