using AutoMapper;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Model;
using ShareBook.Data.Entities.Book.Out;
using ShareBook.Data.Entities.User.Model;
using ShareBook.Data.Entities.User.Out;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Book.Out;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;
using ShareBook.VM.User.Out;

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
            CreateMap<BookModel, BookVM>();
            CreateMap<BookOut, BookOutVM>();
            CreateMap<BookOutById, BookOutByIdVM>();
            #endregion

            #region[ User ]
            CreateMap<UserModel, UserVM>();
            CreateMap<UserOutById, UserOutByIdVM>();
            #endregion

            #region[ ResultService]
            CreateMap<ResultService, ResultServiceVM>();
            #endregion
        }
    }
}
