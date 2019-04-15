using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class UserMap
    {
        public UserMap(EntityTypeBuilder<User> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Name)
                .HasColumnType("varchar(200)")
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.Email)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.Password)
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50)
                    .IsRequired();

            entityBuilder.Property(t => t.PasswordSalt)
                    .HasColumnType("varchar(50)")
                    .HasMaxLength(50)
                    .IsRequired();

            entityBuilder.Property(t => t.Linkedin)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100);

            entityBuilder.Property(t => t.Phone)
                .HasColumnType("varchar(30)")
                .HasMaxLength(30);

            entityBuilder.Property(t => t.HashCodePassword)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200);

            entityBuilder.Property(t => t.HashCodePasswordExpiryDate)
                .HasColumnType("datetime2(7)");

            entityBuilder.HasMany(t => t.BooksDonated)
                .WithOne(b => b.User);

            entityBuilder.Property(t => t.Active)
                .HasDefaultValueSql("1");



        }
    }
}
