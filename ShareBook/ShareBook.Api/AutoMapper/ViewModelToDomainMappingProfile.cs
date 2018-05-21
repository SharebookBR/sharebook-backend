using AutoMapper;
using ShareBook.Data.Entities.User;
using ShareBook.Data.Model;
using ShareBook.VM.Book.Model;
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
            CreateMap<BookVM, Book>();
            #endregion

            #region[ User ]
            CreateMap<UserVM, User>()
                .BeforeMap((src, dest) =>
                dest.Email = src.Email.ToLower());
            #endregion
        }
    }
}
