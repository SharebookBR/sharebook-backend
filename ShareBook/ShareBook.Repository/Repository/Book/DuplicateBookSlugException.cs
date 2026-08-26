namespace ShareBook.Repository
{
    public sealed class DuplicateBookSlugException : Exception
    {
        public DuplicateBookSlugException(string slug, Exception innerException)
            : base($"O slug '{slug}' já está em uso.", innerException)
        {
        }
    }
}
