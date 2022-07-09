using GraphQL.Types;
using ShareBook.Domain;

namespace Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;

public class CategoryType : ObjectGraphType<Category>
{
    public CategoryType()
    {
        Name = "Category";
        Description = "Category Type";
        Field(d => d.Id, nullable: false).Description("Id");
        Field(d => d.Name, nullable: true).Description("Name");

        Field<ListGraphType<BookType>>("books");

    }
}

