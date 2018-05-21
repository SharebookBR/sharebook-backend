using AutoMapper;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book;
using ShareBook.Data.Entities.User;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;

namespace ShareBook.Api.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        : this("Profile")
        {

        }

        protected DomainToViewModelMappingProfile(string profileName)
         : base(profileName)
        {
            #region[ Book ]
            CreateMap<Book, BookVM>();
            #endregion

            #region[ User ]
            CreateMap<User, UserVM>();
            #endregion

            #region[ ResultService]
            CreateMap<ResultService, ResultServiceVM>();
            #endregion
        }
    }
}
