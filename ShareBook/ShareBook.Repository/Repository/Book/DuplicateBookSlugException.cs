namespace ShareBook.Repository;

public sealed class DuplicateBookSlugException(string slug, Exception innerException) : Exception($"O slug '{slug}' já está em uso.", innerException)
{
}
