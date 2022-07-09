using GraphQL;
using GraphQL.Types;
using ShareBook.Repository;

using Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;
using ShareBook.Domain;
using Microsoft.EntityFrameworkCore;

namespace Sharebook.Api.GraphQL.SharebookSchema;

public class SharebookQueries : ObjectGraphType
{
    public SharebookQueries()
    {

        Field<ListGraphType<BookType>>(
            "books",
            arguments: new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "filter" }
            ),
            resolve: context =>
        {
            var filter = context.GetArgument<string>("filter");
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var books = new List<Book>();

            if (!string.IsNullOrEmpty(filter))
                books = dbContext.Books.Where(c => c.Title.Contains(filter)).OrderByDescending(b => b.CreationDate).ToList();
            else
                books = dbContext.Books.OrderByDescending(b => b.CreationDate).ToList();


            return books;
            
        });

        Field<ListGraphType<CategoryType>>(
            "categories",
            arguments: new QueryArguments(
                new QueryArgument<StringGraphType> { Name = "filter" }
            ),
            resolve: context =>
        {
            var filter = context.GetArgument<string>("filter");
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var categories = new List<Category>();          

            if (!string.IsNullOrEmpty(filter))
                categories = dbContext.Categories.Include(c => c.Books).Where(c => c.Name.Contains(filter)).OrderByDescending(b => b.CreationDate).ToList();
            else
                categories = dbContext.Categories.Include(c => c.Books).OrderByDescending(b => b.CreationDate).ToList();

            return categories;

        });
    }
}

