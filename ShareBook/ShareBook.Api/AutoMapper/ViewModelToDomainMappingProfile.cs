using AutoMapper;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;

namespace ShareBook.Api.AutoMapper
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile() : this("Profile") { }

        protected ViewModelToDomainMappingProfile(string profileName) : base(profileName)
        {
            #region [ Book ]
            CreateMap<CreateBookVM, Book>().ReverseMap();
            CreateMap<UpdateBookVM, Book>().ReverseMap();
            CreateMap<DonateBookUserVM, BookUser>().ReverseMap();
            #endregion

            #region [ User ]
            CreateMap<LoginUserVM, User>();

            CreateMap<RegisterUserVM, User>()
                 .ForMember(dest => dest.Address.Street, opt => opt.MapFrom(src => src.Street))
                 .ForMember(dest => dest.Address.Number, opt => opt.MapFrom(src => src.Number))
                 .ForMember(dest => dest.Address.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                 .ForMember(dest => dest.Address.State, opt => opt.MapFrom(src => src.State))
                 .ForMember(dest => dest.Address.City, opt => opt.MapFrom(src => src.City))
                 .ForMember(dest => dest.Address.Neighborhood, opt => opt.MapFrom(src => src.Neighborhood))
                 .ForMember(dest => dest.Address.Country, opt => opt.MapFrom(src => src.Complement))
                 .ForMember(dest => dest.Address.Complement, opt => opt.MapFrom(src => src.Complement));

            CreateMap<UpdateUserVM, User>();
            #endregion
        }
    }
}
