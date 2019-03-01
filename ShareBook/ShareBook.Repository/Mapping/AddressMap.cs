using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class AddressMap
    {
        public AddressMap(EntityTypeBuilder<Address> entityBuilder)
        {
            entityBuilder.Property(t => t.PostalCode)
                   .HasColumnType("varchar(15)")
                   .HasMaxLength(15);

            entityBuilder.Property(t => t.Neighborhood)
                  .HasColumnType("varchar(50)")
                  .HasMaxLength(30);

            entityBuilder.Property(t => t.Complement)
                  .HasColumnType("varchar(50)")
                  .HasMaxLength(30);

            entityBuilder.Property(t => t.Country)
                  .HasColumnType("varchar(50)")
                  .HasMaxLength(30);

            entityBuilder.Property(t => t.City)
                .HasColumnType("varchar(50)")
                .HasMaxLength(30);

            entityBuilder.Property(t => t.State)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30);

            entityBuilder.Property(t => t.Street)
               .HasColumnType("varchar(80)")
               .HasMaxLength(50);

            entityBuilder.Property(t => t.Number)
              .HasColumnType("varchar(10)")
              .HasMaxLength(10);
        }
    }
}
