using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Npgsql;
using NpgsqlTypes;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

namespace ShareBook.Repository
{
    public class BookRepository : RepositoryGeneric<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Book> InsertAsync(Book entity)
        {
            try
            {
                return await base.InsertAsync(entity);
            }
            catch (Exception exception) when (IsUniqueSlugViolation(exception))
            {
                _context.Entry(entity).State = EntityState.Detached;
                throw new DuplicateBookSlugException(entity.Slug, exception);
            }
        }

        public async Task<IList<string>> GetSlugsStartingWithAsync(string baseSlug)
            => await _dbSet
                .AsNoTracking()
                .Where(book => book.Slug.StartsWith(baseSlug))
                .Select(book => book.Slug)
                .ToListAsync();

        public IQueryable<Book> FullTextSearch(string normalizedCriteria, bool includeUnavailable)
        {
            var searchTerm = (normalizedCriteria ?? string.Empty).Trim();
            var books = _dbSet
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
            var lexicalTokens = textTokens
                .Where(token => token.Length >= 2)
                .ToArray();

            if (lexicalTokens.Length == 0)
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

            return rankedBooks
                .Where(candidate => candidate.SearchVector.Matches(
                    EF.Functions.ToTsQuery("simple", prefixQuery)))
                .OrderByDescending(candidate =>
                    EF.Functions.Unaccent(candidate.Title) == exactSearchTerm)
                .ThenByDescending(candidate => EF.Functions.ILike(
                    EF.Functions.Unaccent(candidate.Title),
                    exactSearchTerm + "%"))
                .ThenByDescending(candidate => candidate.SearchVector.RankCoverDensity(
                    EF.Functions.ToTsQuery("simple", prefixQuery)))
                .ThenByDescending(candidate => candidate.Book.CreationDate)
                .Select(candidate => candidate.Book);
        }

        public override async Task<Book> UpdateAsync(Book entity)
        {
         
            _context.Update(entity);

            //imagem eh opcional no update
            if (entity.ImageSlug == null)
                _context.Entry(entity).Property(x => x.ImageSlug).IsModified = false;

            if(entity.Slug == null)
                _context.Entry(entity).Property(x => x.Slug).IsModified = false;

            _context.Entry(entity).Property(x => x.UserId).IsModified = false;
     
            await _context.SaveChangesAsync();

            return entity;
        }

        public override async Task<PagedList<Book>> GetAsync<TKey>(
            Expression<Func<Book, bool>> filter,
            Expression<Func<Book, TKey>> order,
            int page,
            int itemsPerPage,
            bool descending = false)
        {
            var skip = (page - 1) * itemsPerPage;
            var query = _dbSet.Where(filter);
            var total = await query.CountAsync();
            var orderedQuery = descending
                ? query.Include(x => x.BookUsers).Include(x => x.User).OrderByDescending(order)
                : query.Include(x => x.BookUsers).Include(x => x.User).OrderBy(order);

            var result = await orderedQuery
                .Skip(skip)
                .Take(itemsPerPage)
                .ToListAsync();

            return new PagedList<Book>()
            {
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = total,
                Items = result
            };
        }

        private static bool IsUniqueSlugViolation(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is PostgresException postgresException
                    && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                    && postgresException.ConstraintName == "UX_Books_Slug")
                {
                    return true;
                }

                if (current is SqliteException sqliteException
                    && sqliteException.SqliteErrorCode == 19
                    && sqliteException.Message.Contains("Books.Slug", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
