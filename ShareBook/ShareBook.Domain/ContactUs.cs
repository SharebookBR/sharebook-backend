using ShareBook.Domain.Common;
using ShareBook.Helper.Extensions;

namespace ShareBook.Domain;

public class ContactUs : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string FirstName => Name.ToFirstName();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Message { get; set; } = string.Empty;
}
