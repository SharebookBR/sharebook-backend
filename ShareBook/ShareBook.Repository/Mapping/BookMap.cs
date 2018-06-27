using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookMap
    {
        public BookMap(EntityTypeBuilder<Book> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Author)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.Title)
                .HasColumnType("varchar(50)")
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.Image)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Ignore(t => t.ImageBytes);

            entityBuilder.HasOne(t => t.User);

            entityBuilder.HasOne(t => t.Category);
                
        }
    }
}
