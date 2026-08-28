using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service.Home
{
    public class HomeService : IHomeService
    {
        private const int FeaturedPrintedBooks = 15;
        private const int ShowcaseCategories = 3;
        private const int BooksPerCategory = 10;

        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUploadService _uploadService;

        public HomeService(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            IUploadService uploadService)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _uploadService = uploadService;
        }

        public async Task<List<HomeShowcaseBookDTO>> GetFeaturedPrintedBooksAsync()
        {
            var books = await _bookRepository.Get()
                .Where(b => b.Status == BookStatus.Available
                         && b.Type == BookType.Printed)
                .OrderByDescending(b => b.CreationDate)
                .Take(FeaturedPrintedBooks)
                .ToListAsync();

            return books.Select(ToShowcaseBook).ToList();
        }

        public async Task<List<HomeShowcaseCategoryDTO>> GetCategoriesShowcaseAsync()
        {
            // root categories com ebooks diretos
            var directRootIds = _bookRepository.Get()
                .Where(b => b.Status == BookStatus.Available
                         && b.Type == BookType.Eletronic
                         && b.Category.ParentCategoryId == null)
                .Select(b => b.CategoryId);

            // root categories com ebooks em subcategorias
            var subRootIds = _bookRepository.Get()
                .Where(b => b.Status == BookStatus.Available
                         && b.Type == BookType.Eletronic
                         && b.Category.ParentCategoryId != null)
                .Select(b => b.Category.ParentCategoryId.Value);

            var categoryIds = await directRootIds.Union(subRootIds)
                .Distinct()
                .OrderBy(x => EF.Functions.Random())
                .Take(ShowcaseCategories)
                .ToListAsync();

            if (!categoryIds.Any())
                return new List<HomeShowcaseCategoryDTO>();

            var categoryNames = await _categoryRepository.Get()
                .Where(c => categoryIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var result = new List<HomeShowcaseCategoryDTO>();

            foreach (var categoryId in categoryIds)
            {
                if (!categoryNames.TryGetValue(categoryId, out var name))
                    continue;

                // inclui ebooks diretamente na categoria raiz E em suas subcategorias
                var books = await _bookRepository.Get()
                    .Where(b => (b.CategoryId == categoryId || b.Category.ParentCategoryId == categoryId)
                             && b.Status == BookStatus.Available
                             && b.Type == BookType.Eletronic)
                    .OrderBy(b => EF.Functions.Random())
                    .Take(BooksPerCategory)
                    .ToListAsync();

                result.Add(new HomeShowcaseCategoryDTO
                {
                    Id = categoryId,
                    Name = name,
                    Books = books.Select(ToShowcaseBook).ToList()
                });
            }

            return result;
        }

        private HomeShowcaseBookDTO ToShowcaseBook(Book book)
        {
            return new HomeShowcaseBookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Slug = book.Slug,
                ImageUrl = _uploadService.GetImageUrl(book.ImageSlug, "Books", book.ImageVersion),
                ThumbnailUrl = _uploadService.GetBookThumbnailUrl(book.ImageSlug, book.ImageVersion),
                Type = book.Type.ToString()
            };
        }
    }
}
