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

            CreateMap<RegisterUserVM, User>();

            CreateMap<UpdateUserVM, User>();
            #endregion

            #region [ ContactUs ]
            CreateMap<ContactUsVM, ContactUs>();
            #endregion
        }
    }
}
