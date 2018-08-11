using AutoMapper;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;

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
            #endregion

            #region [ User ]
            //CreateMap<User, UserVM>();
            #endregion
        }
    }
}
