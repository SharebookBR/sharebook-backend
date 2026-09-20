namespace ShareBook.Api.ViewModels;

public class ChangePasswordUserVM
{
    public required string NewPassword { get; set; }

    public required string OldPassword { get; set; }
}
