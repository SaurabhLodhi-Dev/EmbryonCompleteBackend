using AutoMapper;
using CleanArchitecture.Application.DTOs.Appointments;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for appointment type mappings.
    /// </summary>
    public class AppointmentTypeProfile : Profile
    {
        public AppointmentTypeProfile()
        {
            CreateMap<AppointmentType, AppointmentTypeDto>();
            CreateMap<CreateAppointmentTypeDto, AppointmentType>();
        }
    }
}
