using GraphQL;
using GraphQL.Types;
using Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;

namespace Sharebook.Api.GraphQL.SharebookSchema;

public class SharebookSchema : Schema
{
    public SharebookSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<SharebookQueries>();

        Mutation = serviceProvider.GetRequiredService<SharebookMutations>();
    }
}

