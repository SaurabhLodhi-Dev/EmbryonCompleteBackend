using AutoMapper;
using CleanArchitecture.Application.DTOs.Appointments;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for Appointment mappings.
    /// </summary>
    public class AppointmentProfile : Profile
    {
        public AppointmentProfile()
        {
            CreateMap<CreateAppointmentDto, Appointment>();

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.AppointmentTypeName, opt => opt.MapFrom(src => src.AppointmentType!.Name))
                .ForMember(dest => dest.SlotDate, opt => opt.MapFrom(src => src.Slot!.SlotDate))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Slot!.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Slot!.EndTime));
        }
    }

}

