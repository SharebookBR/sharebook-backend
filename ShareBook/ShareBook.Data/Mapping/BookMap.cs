using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Data.Model;
namespace ShareBook.Data.Mapping
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
