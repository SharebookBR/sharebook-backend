using GraphQL.Types;
using ShareBook.Domain;
using ShareBook.Repository;

namespace Sharebook.Api.GraphQL;

public class SharebookQueries : ObjectGraphType
{
    public SharebookQueries()
    {

        Field<ListGraphType<BookType>>("books", resolve: context =>
        {
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            return dbContext.Books.OrderByDescending(b => b.CreationDate).ToList();
            
        });
    }
}

