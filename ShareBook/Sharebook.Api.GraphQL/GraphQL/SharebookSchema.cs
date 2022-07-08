using GraphQL.Types;

namespace Sharebook.Api.GraphQL;

public class SharebookSchema : Schema
{
    public SharebookSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<SharebookQueries>();

        Mutation = serviceProvider.GetRequiredService<SharebookMutations>();
    }
}

