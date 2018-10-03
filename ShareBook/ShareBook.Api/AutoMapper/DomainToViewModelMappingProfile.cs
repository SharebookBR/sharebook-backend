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
                 .ForMember(dest => dest.PhoneDonor, opt => opt.MapFrom(src => src.User.Phone));

            CreateMap<BookUser, MyBookRequestVM>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Book.Author))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Description()))
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id));
            #endregion

            #region [ User ]
            CreateMap<User, UserVM>()
                .ForPath(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
                 .ForPath(dest => dest.Number, opt => opt.MapFrom(src => src.Address.Number))
                 .ForPath(dest => dest.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode))
                 .ForPath(dest => dest.State, opt => opt.MapFrom(src => src.Address.State))
                 .ForPath(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                 .ForPath(dest => dest.Neighborhood, opt => opt.MapFrom(src => src.Address.Neighborhood))
                 .ForPath(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Complement))
                 .ForPath(dest => dest.Complement, opt => opt.MapFrom(src => src.Address.Country));
            #endregion
        }
    }
}
