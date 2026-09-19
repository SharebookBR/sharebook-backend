namespace ShareBook.Service
{
    public static class DuplicateEmailExceptionDetector
    {
        public static bool IsDuplicateEmail(Exception ex)
        {
            var details = ex?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(details))
                return false;

            var isDuplicateKey =
                details.Contains("duplicate key value violates unique constraint", StringComparison.OrdinalIgnoreCase) ||
                details.Contains("Cannot insert duplicate key row", StringComparison.OrdinalIgnoreCase) ||
                details.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);

            var isUsersEmailConstraint =
                details.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase) ||
                details.Contains("Users_Email", StringComparison.OrdinalIgnoreCase) ||
                (details.Contains("Users", StringComparison.OrdinalIgnoreCase) &&
                 details.Contains("Email", StringComparison.OrdinalIgnoreCase));

            return isDuplicateKey && isUsersEmailConstraint;
        }
    }
}
