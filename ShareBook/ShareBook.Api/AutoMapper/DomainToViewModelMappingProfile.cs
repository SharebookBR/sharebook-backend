using AutoMapper;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using System;
using System.Globalization;
using System.Linq;

namespace ShareBook.Api.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile() : this("Profile")
        {
        }

        protected DomainToViewModelMappingProfile(string profileName) : base(profileName)
        {
            CreateMap<Category, CategoryVM>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

            CreateMap<Category, BookCategoryVM>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

            #region [ Book ]

            CreateMap<Book, BookVMAdm>()
                 .ForMember(dest => dest.Donor, opt => opt.MapFrom(src => src.User.Name))
                 .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.User.Address.City))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.User.Address.State))
                 .ForMember(dest => dest.Facilitator, opt => opt.MapFrom(src => src.UserFacilitator.Name))
                 .ForMember(dest => dest.FacilitatorNotes, opt => opt.MapFrom(src => src.FacilitatorNotes))
                 .ForMember(dest => dest.PhoneDonor, opt => opt.MapFrom(src => src.User.Phone))
                 .ForMember(dest => dest.DaysInShowcase, opt => opt.MapFrom(src => src.DaysInShowcase()))
                 .ForMember(dest => dest.DaysLate, opt => opt.MapFrom(src => src.DaysLate()))
                 .ForMember(dest => dest.TotalInterested, opt => opt.MapFrom(src => src.TotalInterested()))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                 .ForMember(dest => dest.FreightOption, opt => opt.MapFrom(src => src.FreightOption.HasValue ? src.FreightOption.ToString() : null))
                 .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
                 .ForMember(dest => dest.ChooseDate, opt => opt.MapFrom(src => src.ChooseDate))
                 .ForMember(dest => dest.Winner, opt => opt.MapFrom(src => src.WinnerName()))
                 .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.TrackingNumber))
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                 .ForMember(dest => dest.CategoryInfo, opt => opt.MapFrom(src => src.Category))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Book, BookVM>()
                 .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.User.Address.City))
                 .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.User.Address.State))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                 .ForMember(dest => dest.FreightOption, opt => opt.MapFrom(src => src.FreightOption.HasValue ? src.FreightOption.ToString() : null))
                 .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate))
                 .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                 .ForMember(dest => dest.CategoryInfo, opt => opt.MapFrom(src => src.Category))
                 .ForMember(dest => dest.Donor, opt => opt.MapFrom(src => BuildPublicDonor(src.User)))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<BookUser, MyBookRequestVM>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Book.Author))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.BookStatus, opt => opt.MapFrom(src => src.Book.Status.ToString()))
                .ForMember(dest => dest.TrackingNumber, opt => opt.MapFrom(src => src.Book.TrackingNumber))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId))
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Book.Slug));

            #endregion [ Book ]

            #region [ User ]

            CreateMap<User, UserVM>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));

            CreateMap<User, UserFacilitatorVM>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            CreateMap<AccessHistory, AccessHistoryVM>()
                .ForMember(dest => dest.Profile, opt => opt.MapFrom(src => src.Profile.ToString()))
                .ForMember(dest => dest.VisitingDay, opt => opt.MapFrom(src => src.CreationDate));

            #endregion [ User ]

            #region [ BookUser ]

            CreateMap<BookUser, RequestersListVM>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.RequesterNickName, opt => opt.MapFrom(src => src.NickName))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.User.Location()))
                .ForMember(dest => dest.TotalBooksWon, opt => opt.MapFrom(src => src.User.TotalBooksWon()))
                .ForMember(dest => dest.TotalBooksDonated, opt => opt.MapFrom(src => src.User.TotalBooksDonated()))
                .ForMember(dest => dest.RequestText, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            #endregion [ BookUser ]
        }

        private static BookDonorVM BuildPublicDonor(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new BookDonorVM
            {
                DisplayName = ToTitleCase(AbbreviateName(user.Name)),
                Linkedin = NormalizeLinkedinUrl(user.Linkedin)
            };
        }

        private static string AbbreviateName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var parts = fullName
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            if (parts.Length == 0)
            {
                return null;
            }

            if (parts.Length == 1)
            {
                return parts[0];
            }

            var abbreviatedTail = parts
                .Skip(1)
                .Select(part => $"{part[0]}.")
                .ToArray();

            return $"{parts[0]} {string.Join(" ", abbreviatedTail)}";
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var textInfo = CultureInfo.GetCultureInfo("pt-BR").TextInfo;

            var normalizedParts = value
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part =>
                {
                    if (part.EndsWith(".") && part.Length <= 2)
                    {
                        return char.ToUpperInvariant(part[0]) + ".";
                    }

                    return textInfo.ToTitleCase(part.ToLower());
                });

            return string.Join(" ", normalizedParts);
        }

        private static string NormalizeLinkedinUrl(string linkedin)
        {
            if (string.IsNullOrWhiteSpace(linkedin))
            {
                return null;
            }

            if (linkedin.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || linkedin.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return linkedin;
            }

            return $"https://{linkedin.TrimStart('/')}";
        }
    }
}
