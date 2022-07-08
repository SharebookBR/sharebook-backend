using GraphQL.Types;
using ShareBook.Domain;

namespace Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;

public class BookType : ObjectGraphType<Book>
{
    public BookType()
    {
        Name = "Book";
        Description = "Book Type";
        Field(d => d.Id, nullable: false).Description("Book Id");
        Field(d => d.Title, nullable: true).Description("Book Title");
        Field(d => d.ImageUrl, nullable: true).Description("Book Image URL");
        
        // TODO: descobrir como colocar um outro tipo.
        //Field(d => d.User, nullable: true).Description("Doador");
    }
}

