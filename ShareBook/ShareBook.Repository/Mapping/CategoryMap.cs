using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class CategoryMap : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entityBuilder)
        {

            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

        }
    }
}
