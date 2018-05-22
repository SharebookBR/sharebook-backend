using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;
using ShareBook.Domain.Common;

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
