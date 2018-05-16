using AutoMapper;
using ShareBook.Data.Model;
using ShareBook.VM.Book.In;

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
        }
    }
}
