using GraphQL.Types;

namespace Sharebook.Api.GraphQL.Notes;

public class NotesSchema : Schema
{
    public NotesSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Query = serviceProvider.GetRequiredService<NotesQuery>();
    }
}

