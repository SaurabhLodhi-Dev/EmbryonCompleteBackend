using AutoMapper;
using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Mappings
{
    /// <summary>
    /// AutoMapper profile for Contact Submission mappings.
    /// </summary>
    public class ContactProfile : Profile
    {
        public ContactProfile()
        {
            CreateMap<ContactSubmission, ContactSubmissionDto>()
                .ForMember(dest => dest.Country, opt => opt.Ignore());

            CreateMap<CreateContactSubmissionDto, ContactSubmission>();
        }
    }
}
