using AutoMapper;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Model;
using ShareBook.Data.Entities.Book.Out;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Book.Out;
using ShareBook.VM.Common;

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

            #region[ ResultService]
            CreateMap<ResultService, ResultServiceVM>();
            #endregion
        }
    }
}
