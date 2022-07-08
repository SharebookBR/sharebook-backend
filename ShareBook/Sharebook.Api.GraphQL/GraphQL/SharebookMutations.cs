using GraphQL;
using GraphQL.Types;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;

namespace Sharebook.Api.GraphQL;

public class SharebookMutations : ObjectGraphType
{
    public SharebookMutations()
    {
        Field<BookType>(
            "createBook",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "title" }
            ),
            resolve: context =>
            {
                var title = context.GetArgument<string>("title");
                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

                var category = dbContext.Categories.FirstOrDefault();

                var book = new Book()
                {
                    Author = "Julio Verne",
                    Title = title,
                    FreightOption = FreightOption.World,
                    ImageSlug = "volta-ao-mundo-em-80-dias.jpg",
                    Slug = "volta-ao-mundo-em-80-dias",
                    Status = BookStatus.Available,
                    CreationDate = DateTime.Now.AddDays(-1),
                    ChooseDate = DateTime.Now.AddDays(5),
                    Category = category
                };

                dbContext.Books.Add(book);
                dbContext.SaveChanges();
                return book;
            }
        );

        Field<BookType>(
            "deleteBook",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }
            ),
            resolve: context =>
            {
                var id = context.GetArgument<string>("id");
                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

                // encontrar o livro
                var book = dbContext.Books.FirstOrDefault(b => b.Id == new Guid(id));

                if (book == null)
                    throw new ExecutionError("Livro não encontrado.");

                // deletar
                dbContext.Books.Remove(book);
                dbContext.SaveChanges();

                return book;
            }
        );


    }
}


