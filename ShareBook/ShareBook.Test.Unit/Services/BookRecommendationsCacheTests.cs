using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
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
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

/// <summary>
/// O ranking de recomendações comparava o livro com o catálogo inteiro a cada visita e
/// saturou o pool de conexões no incidente de 2026-11-12. Estes testes protegem o contrato
/// do cache: catálogo compartilhado entre livros e ranking por livro reaproveitado.
/// </summary>
public class BookRecommendationsCacheTests
{
    private readonly Category category = new() { Id = Guid.NewGuid(), Name = "Fantasia" };

    [Fact]
    public async Task Recommendations_ReuseRankingForSameBookWithinCacheWindow()
    {
        await using var context = NewContext();
        var source = await AddBookAsync(context, "Senhor dos Anéis", "Tolkien");
        var close = await AddBookAsync(context, "O Hobbit", "Tolkien");
        var service = CreateService(context, new MemoryCache(new MemoryCacheOptions()));

        var first = await service.GetRecommendationsAsync(source.Id);
        var newcomer = await AddBookAsync(context, "Senhor dos Anéis Parte Dois", "Tolkien");
        var second = await service.GetRecommendationsAsync(source.Id);

        Assert.Equal(new[] { close.Id }, first.Select(b => b.Id));
        Assert.Equal(first.Select(b => b.Id), second.Select(b => b.Id));
        Assert.DoesNotContain(second, b => b.Id == newcomer.Id);
    }

    [Fact]
    public async Task Recommendations_ShareCatalogBetweenDifferentBooks()
    {
        await using var context = NewContext();
        var a = await AddBookAsync(context, "Senhor dos Anéis", "Tolkien");
        var b = await AddBookAsync(context, "O Hobbit", "Tolkien");
        var service = CreateService(context, new MemoryCache(new MemoryCacheOptions()));

        await service.GetRecommendationsAsync(a.Id);
        var late = await AddBookAsync(context, "O Silmarillion", "Tolkien");
        var forB = await service.GetRecommendationsAsync(b.Id);

        // Se o catálogo fosse recarregado para o segundo livro, o "late" apareceria.
        Assert.DoesNotContain(forB, x => x.Id == late.Id);
        Assert.Contains(forB, x => x.Id == a.Id);
    }

    [Fact]
    public async Task Recommendations_WithoutCache_StillReflectCurrentCatalog()
    {
        await using var context = NewContext();
        var source = await AddBookAsync(context, "Senhor dos Anéis", "Tolkien");
        var service = CreateService(context, memoryCache: null);

        Assert.Empty(await service.GetRecommendationsAsync(source.Id));

        var added = await AddBookAsync(context, "O Hobbit", "Tolkien");

        Assert.Contains(await service.GetRecommendationsAsync(source.Id), b => b.Id == added.Id);
    }

    [Fact]
    public async Task Recommendations_ForUnknownBook_ThrowsNotFound()
    {
        await using var context = NewContext();
        var service = CreateService(context, new MemoryCache(new MemoryCacheOptions()));

        var error = await Assert.ThrowsAsync<ShareBookException>(() => service.GetRecommendationsAsync(Guid.NewGuid()));

        Assert.Equal(ShareBookException.Error.NotFound, error.ErrorType);
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private async Task<Book> AddBookAsync(ApplicationDbContext context, string title, string author)
    {
        var book = BookMock.GetLordTheRings();
        book.Id = Guid.NewGuid();
        book.Title = title;
        book.Author = author;
        book.Synopsis = $"{title} de {author}, aventura de fantasia épica.";
        book.Status = BookStatus.Available;
        book.Type = BookType.Eletronic;
        book.Category = category;
        book.CategoryId = category.Id;
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    private static BookService CreateService(ApplicationDbContext context, IMemoryCache? memoryCache)
    {
        var configuration = new ConfigurationBuilder().Build();
        var uploadService = new Mock<IUploadService>();
        return new BookService(new BookRepository(context), context,
            new Mock<IUnitOfWork>().Object, new BookValidator(),
            uploadService.Object, new Mock<IBooksEmailService>().Object, configuration,
            new Mock<NewBookQueue>().Object, new Mock<IEBookService>().Object, new Mock<ICategoryRepository>().Object,
            TimeProvider.System, new Mock<ICurrentUserAccessor>().Object, new Mock<ITagService>().Object,
            memoryCache: memoryCache);
    }
}
