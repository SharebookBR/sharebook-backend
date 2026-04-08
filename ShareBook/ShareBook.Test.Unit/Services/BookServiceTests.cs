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
            bookEmailService = new Mock<IBooksEmailService>();
            bookUserServiceMock = new Mock<IBookUserService>();
            configurationMock = new Mock<IConfiguration>();
            sqsMock = new Mock<NewBookQueue>();

            bookRepositoryMock.Setup(repo => repo.InsertAsync(It.IsAny<Book>())).ReturnsAsync(() =>
            {
                return BookMock.GetLordTheRings();
            });
            uploadServiceMock.Setup(service => service.UploadImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Ok Mocked");
            ebookServiceMock.Setup(service => service.UploadPdfAsync(It.IsAny<Book>())).ReturnsAsync("EBooks/test-book.pdf");
            bookServiceMock.Setup(service => service.InsertAsync(It.IsAny<Book>())).ReturnsAsync(() => new Result<Book>(new Book())).Verifiable();
        }

        [Fact]
        public async Task AddBook()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object,
                unitOfWorkMock.Object, new BookValidator(),
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);
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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);
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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);
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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);
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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);
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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);

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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);

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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);

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
                uploadServiceMock.Object, bookEmailService.Object, configurationMock.Object, sqsMock.Object, ebookServiceMock.Object);

            var result = await service.DeleteAsync(bookId);

            Assert.NotNull(result);
            bookRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<object[]>()), Times.Once);
        }
    }
}
