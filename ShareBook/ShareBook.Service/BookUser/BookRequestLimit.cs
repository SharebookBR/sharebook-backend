using Microsoft.Extensions.Configuration;
using ShareBook.Domain;

namespace ShareBook.Service;

public static class BookRequestLimit
{
    private const int DefaultMaxRequests = 50;

    public static int GetEffective(Book book, IConfiguration configuration)
    {
        if (book.MaxRequestsOverride.HasValue)
            return book.MaxRequestsOverride.Value;

        return int.TryParse(configuration["SharebookSettings:MaxRequestsPerBook"], out var configuredLimit)
            ? configuredLimit
            : DefaultMaxRequests;
    }
}
