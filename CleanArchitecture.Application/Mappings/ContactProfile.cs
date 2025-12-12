

//using AutoMapper;
//using CleanArchitecture.Application.DTOs.Contact;
//using CleanArchitecture.Domain.Entities;

//namespace CleanArchitecture.Application.Mappings
//{
//    /// <summary>
//    /// AutoMapper profile for Contact Submission mappings.
//    /// Maps between Entity and DTOs, handling complex property mappings.
//    /// </summary>
//    public class ContactProfile : Profile
//    {
//        public ContactProfile()
//        {
//            // ==========================================
//            // Entity → Response DTO
//            // ==========================================
//            CreateMap<ContactSubmission, ContactSubmissionDto>()
//       .ConstructUsing(src => new ContactSubmissionDto(
//           src.Id,
//           src.FirstName,
//           src.LastName,
//           src.Email,
//           src.Phone,
//           src.PhoneCountryCode,
//           src.CountryName,
//           src.State,
//           src.City,
//           src.Subject,
//           src.Message,
//           src.IpAddress,
//           src.UserAgent,
//           src.CreatedAt,
//           src.Latitude,        // NEW
//           src.Longitude    // NEW
//       ));

//            CreateMap<CreateContactSubmissionDto, ContactSubmission>()
//                .ForMember(dest => dest.Id, opt => opt.Ignore())
//                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.CountryName));



//            // ==========================================
//            // Create DTO → Entity
//            // ==========================================

//        }
//    }
//}


using AutoMapper;
using CleanArchitecture.Application.DTOs.Contact;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Mappings
{
    public class ContactProfile : Profile
    {
        public ContactProfile()
        {
            // Entity → DTO
            CreateMap<ContactSubmission, ContactSubmissionDto>();

            // DTO → Entity
            //CreateMap<CreateContactSubmissionDto, ContactSubmission>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore())
            //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<CreateContactSubmissionDto, ContactSubmission>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.CountryName));

        }

    }
}
