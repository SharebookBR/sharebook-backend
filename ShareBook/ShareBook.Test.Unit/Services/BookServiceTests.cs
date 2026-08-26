using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.EBook;
using ShareBook.Service.Upload;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookServiceTests
    {
        readonly Mock<IBookService> bookServiceMock;
        readonly Mock<IUploadService> uploadServiceMock;
        readonly Mock<IEBookService> ebookServiceMock;
        readonly Mock<IBookRepository> bookRepositoryMock;
        readonly Mock<ICategoryRepository> categoryRepositoryMock;
        readonly Mock<IBooksEmailService> bookEmailService;
        readonly Mock<IUnitOfWork> unitOfWorkMock;
        readonly Mock<IBookUserService> bookUserServiceMock;
        readonly Mock<IConfiguration> configurationMock;

        readonly Mock<NewBookQueue> sqsMock;

        public BookServiceTests()
        {
            // Definindo quais serão as classes mockadas
            bookServiceMock = new Mock<IBookService>();
            uploadServiceMock = new Mock<IUploadService>();
            ebookServiceMock = new Mock<IEBookService>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            bookRepositoryMock = new Mock<IBookRepository>();
            categoryRepositoryMock = new Mock<ICategoryRepository>();
            bookEmailService = new Mock<IBooksEmailService>();
            bookUserServiceMock = new Mock<IBookUserService>();
            configurationMock = new Mock<IConfiguration>();
            sqsMock = new Mock<NewBookQueue>();

            bookRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Book>())).ReturnsAsync(() =>
            {
                return BookMock.GetLordTheRings();
            });
            bookRepositoryMock.Setup(repo => repo.Get()).Returns(Array.Empty<Book>().AsQueryable());
            bookRepositoryMock.Setup(repo => repo.GetSlugsStartingWithAsync(It.IsAny<string>()))
                .ReturnsAsync(Array.Empty<string>());
            uploadServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Ok Mocked");
            ebookServiceMock.Setup(service => service.UploadPdfAsync(It.IsAny<Book>())).ReturnsAsync("EBooks/test-book.pdf");
            categoryRepositoryMock.Setup(repo => repo.Get()).Returns(new[] { new Category { Id = Guid.NewGuid(), Name = "Leaf" } }.AsQueryable());
            bookServiceMock.Setup(service => service.InsertAsync(It.IsAny<Book>())).ReturnsAsync(() => new Result<Book>(new Book())).Verifiable();
        }

        // O UpdateBookVM nao carrega ImageSlug, entao um PUT sem imagem nova chega
        // ao service com ImageSlug nulo. Se o service copiar isso cegamente, o livro
        // perde a capa. Investigado a partir do incidente de 20/08/2026.
        [Fact]
        public async Task UpdateBookWithoutNewImage_ShouldKeepImageSlug()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var savedBook = BookMock.GetLordTheRings();
            savedBook.Id = Guid.NewGuid();
            savedBook.ImageSlug = "lotr.png";

            bookRepositoryMock.Setup(repo => repo.FindAsync(It.IsAny<object[]>())).ReturnsAsync(savedBook);
            bookRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).ReturnsAsync((Book book) => book);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            Result<Book> result = await service.UpdateAsync(new Book()
            {
                Id = savedBook.Id,
                Title = savedBook.Title,
                Author = savedBook.Author,
                CategoryId = savedBook.CategoryId,
                Synopsis = savedBook.Synopsis,
                FreightOption = FreightOption.City,
                ImageName = "",
                ImageBytes = null
            });

            Assert.Equal("lotr.png", result.Value.ImageSlug);
        }

        [Fact]
        public async Task UpdateBook_WhenTitleChanges_ShouldKeepPublicSlug()
        {
            var categoryId = Guid.NewGuid();
            var savedBook = BookMock.GetLordTheRings();
            savedBook.Id = Guid.NewGuid();
            savedBook.CategoryId = categoryId;
            savedBook.Slug = "lord-of-the-rings";

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = categoryId, Name = "Leaf" } }.AsQueryable());
            bookRepositoryMock.Setup(repo => repo.FindAsync(savedBook.Id)).ReturnsAsync(savedBook);
            bookRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).ReturnsAsync((Book book) => book);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.UpdateAsync(new Book
            {
                Id = savedBook.Id,
                Title = "The Lord of the Rings",
                Author = savedBook.Author,
                CategoryId = categoryId,
                Synopsis = savedBook.Synopsis,
                FreightOption = FreightOption.City
            });

            Assert.True(result.Success);
            Assert.Equal("lord-of-the-rings", result.Value.Slug);
        }

        [Fact]
        public async Task AddBooksWithSameTitle_ShouldUseCopySuffixes()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var categoryId = Guid.NewGuid();
            var insertedBooks = new System.Collections.Generic.List<Book>();

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = categoryId, Name = "Leaf" } }.AsQueryable());
            bookRepositoryMock
                .Setup(repo => repo.GetSlugsStartingWithAsync(It.IsAny<string>()))
                .ReturnsAsync(() => insertedBooks.Select(book => book.Slug).ToList());
            bookRepositoryMock
                .Setup(repo => repo.InsertAsync(It.IsAny<Book>()))
                .ReturnsAsync((Book book) =>
                {
                    insertedBooks.Add(book);
                    return book;
                });

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var first = new Book
            {
                Title = "O Pequeno Príncipe",
                Author = "Antoine de Saint-Exupéry",
                ImageName = "first.png",
                ImageBytes = Encoding.UTF8.GetBytes("FIRST"),
                FreightOption = FreightOption.City,
                CategoryId = categoryId,
                Type = BookType.Printed
            };
            var second = new Book
            {
                Title = first.Title,
                Author = first.Author,
                ImageName = "second.png",
                ImageBytes = Encoding.UTF8.GetBytes("SECOND"),
                FreightOption = FreightOption.City,
                CategoryId = categoryId,
                Type = BookType.Printed
            };

            await service.InsertAsync(first);
            await service.InsertAsync(second);

            Assert.Equal(2, insertedBooks.Count);
            Assert.Equal("o-pequeno-principe", insertedBooks[0].Slug);
            Assert.Equal("o-pequeno-principe_copy1", insertedBooks[1].Slug);
        }

        [Fact]
        public async Task AddBook_WhenSlugIsTakenConcurrently_ShouldRetryWithNextCopy()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var categoryId = Guid.NewGuid();
            var existingBooks = new System.Collections.Generic.List<Book>();
            var insertAttempts = 0;

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = categoryId, Name = "Leaf" } }.AsQueryable());
            bookRepositoryMock
                .Setup(repo => repo.GetSlugsStartingWithAsync(It.IsAny<string>()))
                .ReturnsAsync(() => existingBooks.Select(book => book.Slug).ToList());
            bookRepositoryMock
                .Setup(repo => repo.InsertAsync(It.IsAny<Book>()))
                .ReturnsAsync((Book book) =>
                {
                    insertAttempts++;
                    if (insertAttempts == 1)
                    {
                        existingBooks.Add(new Book { Slug = book.Slug });
                        throw new DuplicateBookSlugException(book.Slug, new Exception("simulated race"));
                    }

                    existingBooks.Add(book);
                    return book;
                });

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.InsertAsync(new Book
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ImageName = "clean-code.png",
                ImageBytes = Encoding.UTF8.GetBytes("IMAGE"),
                FreightOption = FreightOption.City,
                CategoryId = categoryId,
                Type = BookType.Printed
            });

            Assert.True(result.Success);
            Assert.Equal(2, insertAttempts);
            Assert.Equal("clean-code_copy1", result.Value.Slug);
        }

        [Fact]
        public async Task AddBook()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);
            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageName = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                FreightOption = FreightOption.City,
                CategoryId = Guid.NewGuid(),
                Type = BookType.Printed
            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task AddEBookWithPdf()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);
            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ImageName = "clean-code.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                Type = BookType.Eletronic,
                PdfBytes = Encoding.UTF8.GetBytes("PDF_CONTENT_BASE64")
            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task AddEBookWithoutPdf_ShouldFail()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);
            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ImageName = "clean-code.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                Type = BookType.Eletronic
            });
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task AddPrintedBookWithoutFreight_ShouldFail()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);
            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageName = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                Type = BookType.Printed
            });
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task EBookShouldNotRequireFreight()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);
            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ImageName = "clean-code.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                Type = BookType.Eletronic,
                PdfBytes = Encoding.UTF8.GetBytes("PDF_CONTENT_BASE64")
            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task AddDuplicateEBook_ShouldFail()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();

            // Simula que já existe um ebook com o mesmo título e autor no banco
            bookRepositoryMock
                .Setup(repo => repo.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>()))
                .ReturnsAsync(true);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ImageName = "clean-code.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                Type = BookType.Eletronic,
                PdfBytes = Encoding.UTF8.GetBytes("PDF_CONTENT_BASE64")
            });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Já existe um e-book com este título e autor no catálogo.", result.Messages);
        }

        [Fact]
        public async Task AddDuplicatePrintedBook_ShouldNotCheckForDuplicateEBook()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();

            // AnyAsync nunca deve ser chamado para livros físicos
            bookRepositoryMock
                .Setup(repo => repo.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>()))
                .ReturnsAsync(true);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageName = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                FreightOption = FreightOption.City,
                CategoryId = Guid.NewGuid(),
                Type = BookType.Printed
            });

            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task InsertBook_WithParentCategory_ShouldFail()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var parentCategoryId = Guid.NewGuid();

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = Guid.NewGuid(), ParentCategoryId = parentCategoryId } }.AsQueryable());

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            Result<Book> result = await service.InsertAsync(new Book()
            {
                Title = "Livro Teste",
                Author = "Autor Teste",
                ImageName = "teste.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = parentCategoryId,
                FreightOption = FreightOption.City,
                Type = BookType.Printed
            });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Selecione uma subcategoria final", result.Messages[0]);
        }

        [Fact]
        public async Task UpdateBook_WithParentCategory_ShouldFail()
        {
            var parentCategoryId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var savedBook = new Book
            {
                Id = bookId,
                Title = "Livro Original",
                Author = "Autor",
                CategoryId = Guid.NewGuid(),
                Synopsis = "x",
                Slug = "livro-original"
            };

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = Guid.NewGuid(), ParentCategoryId = parentCategoryId } }.AsQueryable());

            bookRepositoryMock.Setup(repo => repo.FindAsync(bookId)).ReturnsAsync(savedBook);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.UpdateAsync(new Book
            {
                Id = bookId,
                Title = "Livro Original",
                Author = "Autor",
                CategoryId = parentCategoryId,
                Synopsis = "x"
            });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Selecione uma subcategoria final", result.Messages[0]);
            bookRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }
        [Fact]
        public async Task UpdateBook_WhenCoverExtensionChanges_ShouldUploadUsingNewSlugAndDeleteOldFile()
        {
            var bookId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var savedBook = new Book
            {
                Id = bookId,
                Title = "Livro Original",
                Author = "Autor",
                CategoryId = categoryId,
                Synopsis = "x",
                Slug = "livro-original",
                ImageSlug = "livro-original.jpg"
            };

            categoryRepositoryMock
                .Setup(repo => repo.Get())
                .Returns(new[] { new Category { Id = categoryId, Name = "Leaf" } }.AsQueryable());

            bookRepositoryMock.Setup(repo => repo.FindAsync(bookId)).ReturnsAsync(savedBook);
            bookRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Book>())).ReturnsAsync((Book b) => b);

            uploadServiceMock
                .Setup(service => service.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("Ok Mocked");

            uploadServiceMock
                .Setup(service => service.DeleteFileIfExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.UpdateAsync(new Book
            {
                Id = bookId,
                Title = "Livro Original",
                Author = "Autor",
                CategoryId = categoryId,
                Synopsis = "x",
                ImageName = "nova-capa.png",
                ImageBytes = Encoding.UTF8.GetBytes("PNG_BYTES")
            });

            Assert.NotNull(result);
            Assert.True(result.Success);

            uploadServiceMock.Verify(service => service.UploadImageAsync(
                It.IsAny<byte[]>(),
                "livro-original.png",
                "Books"), Times.Once);

            uploadServiceMock.Verify(service => service.DeleteFileIfExistsAsync(
                "livro-original.jpg",
                "Books"), Times.Once);
        }

        [Fact]
        public async Task DeleteEBook_ShouldTryDeleteAssetsAndDeleteDbRecord()
        {
            var bookId = Guid.NewGuid();
            var savedBook = new Book
            {
                Id = bookId,
                Title = "Cloud",
                Author = "Sharebook",
                Type = BookType.Eletronic,
                ImageSlug = "cloud.jpg",
                EBookPdfPath = "ebooks/cloud.pdf",
                CategoryId = Guid.NewGuid(),
                Synopsis = "x"
            };

            bookRepositoryMock.Setup(repo => repo.FindAsync(It.IsAny<object[]>())).ReturnsAsync(savedBook);
            bookRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<object[]>())).Returns(Task.CompletedTask).Verifiable();
            uploadServiceMock.Setup(service => service.DeleteFileIfExistsAsync("cloud.jpg", "Books")).Returns(Task.CompletedTask).Verifiable();
            ebookServiceMock.Setup(service => service.DeletePdfAsync(savedBook)).Returns(Task.CompletedTask).Verifiable();

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.DeleteAsync(bookId);

            Assert.NotNull(result);
            bookRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<object[]>()), Times.Once);
            uploadServiceMock.Verify(service => service.DeleteFileIfExistsAsync("cloud.jpg", "Books"), Times.Once);
            ebookServiceMock.Verify(service => service.DeletePdfAsync(savedBook), Times.Once);
        }

        [Fact]
        public async Task DeleteBook_ShouldDeleteDbRecordEvenWhenAssetCleanupFails()
        {
            var bookId = Guid.NewGuid();
            var savedBook = new Book
            {
                Id = bookId,
                Title = "Cloud",
                Author = "Sharebook",
                Type = BookType.Eletronic,
                ImageSlug = "cloud.jpg",
                EBookPdfPath = "ebooks/cloud.pdf",
                CategoryId = Guid.NewGuid(),
                Synopsis = "x"
            };

            bookRepositoryMock.Setup(repo => repo.FindAsync(It.IsAny<object[]>())).ReturnsAsync(savedBook);
            bookRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<object[]>())).Returns(Task.CompletedTask).Verifiable();
            uploadServiceMock.Setup(service => service.DeleteFileIfExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("img error"));
            ebookServiceMock.Setup(service => service.DeletePdfAsync(It.IsAny<Book>())).ThrowsAsync(new Exception("pdf error"));

            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object, categoryRepositoryMock.Object);

            var result = await service.DeleteAsync(bookId);

            Assert.NotNull(result);
            bookRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<object[]>()), Times.Once);
        }
    }
}
