using GraphQL.Types;
using ShareBook.Repository;

using Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;

namespace Sharebook.Api.GraphQL.SharebookSchema;

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

