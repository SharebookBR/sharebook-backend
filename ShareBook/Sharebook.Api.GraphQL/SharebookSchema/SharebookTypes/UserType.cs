using GraphQL.Types;
using ShareBook.Domain;

namespace Sharebook.Api.GraphQL.SharebookSchema.SharebookTypes;

public class UserType : ObjectGraphType<User>
{
    public UserType()
    {
        Name = "User";
        Description = "User Type";
        Field(d => d.Id, nullable: false).Description("Id");
        Field(d => d.Name, nullable: true).Description("Name");
        Field(d => d.Email, nullable: true).Description("Email");
        
        // TODO: descobrir como colocar um outro tipo.
        //Field(d => d.User, nullable: true).Description("Doador");
    }
}

