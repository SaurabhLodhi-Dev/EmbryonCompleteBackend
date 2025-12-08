//using CleanArchitecture.Application.DTOs.Appointments;
//using CleanArchitecture.Application.Interfaces;
//using CleanArchitecture.WebApi.Middlewares;
//using Microsoft.AspNetCore.Mvc;

//namespace CleanArchitecture.WebApi.Controllers
//{
//    /// <summary>
//    /// Handles booking and retrieving appointments.
//    /// Automatically injects IP address using Geo middleware.
//    /// </summary>
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AppointmentsController : ControllerBase
//    {
//        private readonly IAppointmentService _service;

//        public AppointmentsController(IAppointmentService service)
//        {
//            _service = service;
//        }

//        /// <summary>
//        /// Returns all booked appointments.
//        /// </summary>
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll()
//        {
//            var list = await _service.GetAllAsync();
//            return Ok(list);
//        }

//        /// <summary>
//        /// Returns a single appointment by ID.
//        /// </summary>
//        [HttpGet("{id:guid}")]
//        public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
//        {
//            var result = await _service.GetByIdAsync(id);
//            return Ok(result);
//        }

//        /// <summary>
//        /// Books a new appointment.
//        /// Automatically attaches user's IP address.
//        /// </summary>
//        [HttpPost]
//        public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
//        {
//            // Get Geo-IP data from middleware (only IP used here)
//            var geo = HttpContext.Items["GeoInfo"] as GeoInfo;

//            // Update dto with auto-detected IP
//            dto = dto with
//            {
//                IpAddress = geo?.ip
//            };

//            var created = await _service.CreateAsync(dto);

//            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
//        }
//    }
//}
