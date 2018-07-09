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
            //CreateMap<BookVM, Book>();
            #endregion

            #region [ User ]
            //CreateMap<UserVM, User>()
            //    .BeforeMap((src, dest) =>
            //    dest.Email = src.Email.ToLower());

            CreateMap<UpdateUserVM, User>();
            #endregion
        }
    }
}
