using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class BookRepository : IBookRepository
{
    // Termos de 1 caractere que representam uma busca real (linguagens de
    // programação). Casam por lexema EXATO (sem prefixo) contra título +
    // categoria — nunca contra autor/sinopse, para evitar iniciais de autor
    // (ex.: "J. R. King") e menções soltas no texto.
    private static readonly string[] ExactSingleCharTerms = { "r", "c" };

    private readonly ApplicationDbContext _context;
    private readonly EntityCrud<Book> _crud;

    public BookRepository(ApplicationDbContext context)
    {
        _context = context;
        _crud = new EntityCrud<Book>(context);
    }

    public IQueryable<Book> Get() => _crud.Get();

    public Task<PagedList<Book>> GetAsync<TKey>(Expression<Func<Book, bool>> filter, Expression<Func<Book, TKey>> order)
        => _crud.GetAsync(filter, order);

    public Task<Book> UpdateAsync(Book entity) => _crud.UpdateAsync(entity);

    public async Task<IList<string>> GetSlugsStartingWithAsync(string baseSlug)
        => await _context.Books
            .AsNoTracking()
            .Where(book => book.Slug.StartsWith(baseSlug))
            .Select(book => book.Slug)
            .ToListAsync();

    public IQueryable<Book> FullTextSearch(string normalizedCriteria, bool includeUnavailable)
    {
        var searchTerm = (normalizedCriteria ?? string.Empty).Trim();
        var books = _context.Books
            .AsNoTracking()
            .Where(book => includeUnavailable || book.Status == BookStatus.Available);

        if (string.IsNullOrWhiteSpace(searchTerm))
            return books.Where(_ => false);

        var tokens = searchTerm
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        var printedTerms = new[] { "fisico", "impresso", "printed", "physical" };
        var electronicTerms = new[] { "ebook", "digital", "eletronico", "electronic" };
        var requiresPrinted = tokens.Any(token => printedTerms.Contains(token));
        var requiresElectronic = tokens.Any(token => electronicTerms.Contains(token));

        if (requiresPrinted && requiresElectronic)
            return books.Where(_ => false);

        if (requiresPrinted)
            books = books.Where(book => book.Type == BookType.Printed);
        else if (requiresElectronic)
            books = books.Where(book => book.Type == BookType.Eletronic);

        var textTokens = tokens
            .Where(token => !printedTerms.Contains(token) && !electronicTerms.Contains(token))
            .ToArray();
        var exactTokens = textTokens
            .Where(token => ExactSingleCharTerms.Contains(token))
            .ToArray();
        var lexicalTokens = textTokens
            .Where(token => token.Length >= 2 && !ExactSingleCharTerms.Contains(token))
            .ToArray();

        if (exactTokens.Length == 0 && lexicalTokens.Length == 0)
        {
            return requiresPrinted || requiresElectronic
                ? books.OrderByDescending(book => book.CreationDate)
                : books.Where(_ => false);
        }

        var exactSearchTerm = string.Join(" ", textTokens);

        // SQLite e InMemory não oferecem o FTS do PostgreSQL. Este caminho existe
        // somente para testes e ambientes locais alternativos; produção usa Npgsql.
        if (!_context.Database.IsNpgsql())
        {
            var loweredTerm = exactSearchTerm.ToLowerInvariant();
            return books
                .Where(book =>
                    book.Title.ToLower().Contains(loweredTerm)
                    || book.Author.ToLower().Contains(loweredTerm)
                    || book.Category.Name.ToLower().Contains(loweredTerm)
                    || (book.Category.ParentCategory != null
                        && book.Category.ParentCategory.Name.ToLower().Contains(loweredTerm))
                    || (book.Synopsis != null && book.Synopsis.ToLower().Contains(loweredTerm)))
                .OrderByDescending(book => book.CreationDate);
        }

        var prefixQuery = string.Join(" & ", lexicalTokens.Select(token => $"{token}:*"));
        var exactQuery = string.Join(" & ", exactTokens);
        var rankQuery = string.Join(" & ",
            lexicalTokens.Select(token => $"{token}:*").Concat(exactTokens));

        var searchableBooks = books.Select(book => new
        {
            Book = book,
            Title = book.Title.ToLower()
                .Replace("c++", "cplusplus")
                .Replace("c#", "csharp")
                .Replace("f#", "fsharp")
                .Replace(".net", "dotnet"),
            Author = book.Author,
            LeafCategory = book.Category.Name,
            ParentCategory = book.Category.ParentCategory == null
                ? string.Empty
                : book.Category.ParentCategory.Name,
            Synopsis = (book.Synopsis ?? string.Empty).ToLower()
                .Replace("c++", "cplusplus")
                .Replace("c#", "csharp")
                .Replace("f#", "fsharp")
                .Replace(".net", "dotnet")
        });

        var rankedBooks = searchableBooks.Select(candidate => new
        {
            candidate.Book,
            candidate.Title,
            TitleCategoryVector = EF.Functions
                .ToTsVector("simple", EF.Functions.Unaccent(candidate.Title))
                .SetWeight(NpgsqlTsVector.Lexeme.Weight.A)
                .Concat(EF.Functions
                    .ToTsVector("simple", EF.Functions.Unaccent(candidate.LeafCategory))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.C))
                .Concat(EF.Functions
                    .ToTsVector(
                        "simple",
                        EF.Functions.Unaccent(candidate.ParentCategory))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.C)),
            SearchVector = EF.Functions
                .ToTsVector("simple", EF.Functions.Unaccent(candidate.Title))
                .SetWeight(NpgsqlTsVector.Lexeme.Weight.A)
                .Concat(EF.Functions
                    .ToTsVector("simple", EF.Functions.Unaccent(candidate.Author))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.B))
                .Concat(EF.Functions
                    .ToTsVector("simple", EF.Functions.Unaccent(candidate.LeafCategory))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.C))
                .Concat(EF.Functions
                    .ToTsVector(
                        "simple",
                        EF.Functions.Unaccent(candidate.ParentCategory))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.C))
                .Concat(EF.Functions
                    .ToTsVector("simple", EF.Functions.Unaccent(candidate.Synopsis))
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.D))
        });

        var filtered = rankedBooks;

        if (lexicalTokens.Length > 0)
            filtered = filtered.Where(candidate => candidate.SearchVector.Matches(
                EF.Functions.ToTsQuery("simple", prefixQuery)));

        if (exactTokens.Length > 0)
            filtered = filtered.Where(candidate => candidate.TitleCategoryVector.Matches(
                EF.Functions.ToTsQuery("simple", exactQuery)));

        return filtered
            .OrderByDescending(candidate =>
                EF.Functions.Unaccent(candidate.Title) == exactSearchTerm)
            .ThenByDescending(candidate => EF.Functions.ILike(
                EF.Functions.Unaccent(candidate.Title),
                exactSearchTerm + "%"))
            .ThenByDescending(candidate => candidate.SearchVector.RankCoverDensity(
                EF.Functions.ToTsQuery("simple", rankQuery)))
            .ThenByDescending(candidate => candidate.Book.CreationDate)
            .Select(candidate => candidate.Book);
    }
}
