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
            #endregion

            #region [ User ]
            CreateMap<LoginUserVM, User>();

            CreateMap<RegisterUserVM, User>();

            CreateMap<UpdateUserVM, User>();
            #endregion
        }
    }
}
