namespace ShareBook.Api.ViewModels;

public class ChangeUserPasswordByHashCodeVM
{
    public required string HashCodePassword { get; set; }

    public required string NewPassword { get; set; }
}
