using AutoMapper;
using CleanArchitecture.Application.DTOs.Appointments;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces;

namespace CleanArchitecture.Application.Services
{
    /// <summary>
    /// Handles logic related to booking and retrieving appointments.
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentSlotRepository _slotRepository;
        private readonly IAppointmentTypeRepository _typeRepository;
        private readonly IMapper _mapper;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAppointmentSlotRepository slotRepository,
            IAppointmentTypeRepository typeRepository,
            IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _slotRepository = slotRepository;
            _typeRepository = typeRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a single appointment by ID.
        /// </summary>
        public async Task<AppointmentDto> GetByIdAsync(Guid id)
        {
            var entity = await _appointmentRepository.GetByIdAsync(id)
                ?? throw new Exception("Appointment not found.");

            // AutoMapper will map slot & type inside AppointmentProfile
            var dto = _mapper.Map<AppointmentDto>(entity);

            return dto;
        }

        /// <summary>
        /// Returns all booked appointments.
        /// </summary>
        public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
        {
            var list = await _appointmentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AppointmentDto>>(list);
        }

        /// <summary>
        /// Books a new appointment.
        /// </summary>
        public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
        {
            // Validate Slot
            var slot = await _slotRepository.GetByIdAsync(dto.SlotId!.Value)
                ?? throw new Exception("Invalid slot selected.");

            if (slot.IsBooked == true)
                throw new Exception("This slot is already booked.");

            // Validate Appointment Type
            var type = await _typeRepository.GetByIdAsync(dto.AppointmentTypeId!.Value)
                ?? throw new Exception("Invalid appointment type.");

            // Map CreateAppointmentDto → Appointment entity
            var entity = _mapper.Map<Appointment>(dto);

            // Save appointment
            var created = await _appointmentRepository.AddAsync(entity);

            // Mark slot as booked
            await _slotRepository.MarkSlotAsBookedAsync(slot.Id);

            // Map Appointment → AppointmentDto
            var response = _mapper.Map<AppointmentDto>(created);

            return response;
        }
    }
}
