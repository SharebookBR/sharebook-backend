﻿using GraphQL.Types;

namespace Sharebook.Api.GraphQL.Notes;

public class NoteType : ObjectGraphType<Note>
{
    public NoteType()
    {
        Name = "Note";
        Description = "Note Type";
        Field(d => d.Id, nullable: false).Description("Note Id");
        Field(d => d.Message, nullable: true).Description("Note Message");
    }
}

