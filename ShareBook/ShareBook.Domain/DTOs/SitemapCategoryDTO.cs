namespace ShareBook.Domain.DTOs
{
    public class SitemapCategoryDTO
    {
        public string Name { get; set; }
        public string ParentCategoryName { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
