namespace ShareBook.Api.ViewModels;

public class ContactUsVM
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Message { get; set; }
    public string? RecaptchaReactive { get; set; }
}
