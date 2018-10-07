using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class CategoryMap
    {
        public CategoryMap(EntityTypeBuilder<Category> entityBuilder)
        {

            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Name)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            
        }
    }
}
