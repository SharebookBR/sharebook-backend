using AutoMapper;
using ShareBook.Data.Entities.User;
using ShareBook.Data.Model;
using ShareBook.VM.Book.In;
using ShareBook.VM.User.Model;

namespace ShareBook.Api.AutoMapper
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        : this("Profile")
        {

        }
        protected ViewModelToDomainMappingProfile(string profileName)
        : base(profileName)
        {
            #region[ Book ]
            CreateMap<BookInVM, Book>();
            #endregion

            #region[ User ]
            CreateMap<UserVM, User>();
            #endregion
        }
    }
}
