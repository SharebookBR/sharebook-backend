using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookMap
    {
        public BookMap(EntityTypeBuilder<Book> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);
            entityBuilder.Property(t => t.Name);
        }
    }
}
