using GraphQL.Types;
using ShareBook.Repository;

namespace Sharebook.Api.GraphQL.Notes;

public class BooksQuery : ObjectGraphType
{
    public BooksQuery()
    {
        Field<ListGraphType<BookType>>("books", resolve: context =>
        {
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            return dbContext.Books.ToList();

        });
    }
}

