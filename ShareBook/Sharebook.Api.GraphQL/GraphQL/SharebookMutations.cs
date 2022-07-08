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
                var notesContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

                var category = notesContext.Categories.FirstOrDefault();

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

                notesContext.Books.Add(book);
                notesContext.SaveChanges();
                return book;
            }
        );
    }
}


