using System.Collections.Generic;

namespace ShareBook.Service.Authorization;

public class Permissions
{
    public enum Permission
    {
        CreateBook,
        UpdateBook,
        DeleteBook,
        ApproveBook,
        DonateBook
    }

    public static List<Permission> AdminPermissions { get; } = new List<Permission>() { Permission.ApproveBook, Permission.DonateBook };
}
