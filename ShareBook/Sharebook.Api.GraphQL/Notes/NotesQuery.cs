using GraphQL.Types;
using ShareBook.Domain;
using ShareBook.Repository;

namespace Sharebook.Api.GraphQL.Notes;

public class NotesQuery : ObjectGraphType
{
    public NotesQuery()
    {
        Field<ListGraphType<NoteType>>("notes", resolve: context => new List<Note> {
            new Note { Id = Guid.NewGuid(), Message = "Hello World!" },
            new Note { Id = Guid.NewGuid(), Message = "Hello World! How are you?" }
        });

        Field<ListGraphType<BookType>>("books", resolve: context =>
        {
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            return dbContext.Books.OrderByDescending(b => b.CreationDate).ToList();
            
        });
    }
}

