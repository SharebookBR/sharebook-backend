using AutoMapper;

namespace ShareBook.Api.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile() : this("Profile") { }

        protected DomainToViewModelMappingProfile(string profileName) : base(profileName)
        {
            #region [ Book ]
            //CreateMap<Book, BookVM>();
            #endregion

            #region [ User ]
            //CreateMap<User, UserVM>();
            #endregion
        }
    }
}
