using AutoMapper;
using CleanArchitecture.Application.DTOs.Appointments;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for appointment slot mappings.
    /// </summary>
    public class AppointmentSlotProfile : Profile
    {
        public AppointmentSlotProfile()
        {
            CreateMap<AppointmentSlot, AppointmentSlotDto>();
            CreateMap<CreateAppointmentSlotDto, AppointmentSlot>();
        }
    }
}
