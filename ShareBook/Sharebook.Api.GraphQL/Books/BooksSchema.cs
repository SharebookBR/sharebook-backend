using GraphQL.Types;

namespace Sharebook.Api.GraphQL.Notes;

public class BooksSchema : Schema
{
    public BooksSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<BooksQuery>();
    }
}

