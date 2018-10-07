using AutoMapper;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Helper.Extensions;

namespace ShareBook.Api.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile() : this("Profile") { }

        protected DomainToViewModelMappingProfile(string profileName) : base(profileName)
        {
            #region [ Book ]
            CreateMap<Book, BooksVM>()
                 .ForMember(dest => dest.Donor, opt => opt.MapFrom(src => src.User.Name))
                 .ForMember(dest => dest.Donated, opt => opt.MapFrom(src => src.Donated()))
                 .ForMember(dest => dest.PhoneDonor, opt => opt.MapFrom(src => src.User.Phone))
                 .ForMember(dest => dest.DaysInShowcase, opt => opt.MapFrom(src => src.DaysInShowcase()))
                 .ForMember(dest => dest.TotalInterested, opt => opt.MapFrom(src => src.TotalInterested()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status().Description()));

            CreateMap<BookUser, MyBookRequestVM>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Book.Author))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Description()))
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id));
            #endregion

            #region [ User ]
            //CreateMap<User, UserVM>();
            #endregion
        }
    }
}
