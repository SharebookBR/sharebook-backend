using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class AddressMap : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> entityBuilder)
        {
            entityBuilder.Property(t => t.PostalCode)
                   .HasMaxLength(15);

            entityBuilder.Property(t => t.Neighborhood)
                  .HasMaxLength(50);

            entityBuilder.Property(t => t.Complement)
                  .HasMaxLength(50);

            entityBuilder.Property(t => t.Country)
                  .HasMaxLength(50);

            entityBuilder.Property(t => t.City)
                .HasMaxLength(50);

            entityBuilder.Property(t => t.State)
                .HasMaxLength(30);

            entityBuilder.Property(t => t.Street)
               .HasMaxLength(80);

            entityBuilder.Property(t => t.Number)
              .HasMaxLength(10);
        }
    }
}
