using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Name)
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.Email)
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.HasIndex(t => t.Email)
                .IsUnique();

            entityBuilder.Property(t => t.Password)
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.PasswordSalt)
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.Linkedin)
                .HasMaxLength(100);

            entityBuilder.Property(t => t.Phone)
                .HasMaxLength(30);

            entityBuilder.Property(t => t.HashCodePassword)
                .HasMaxLength(200);

            // Removido HasColumnType específico - EF Core escolhe automaticamente

            entityBuilder.HasMany(t => t.BooksDonated)
                .WithOne(b => b.User);

            entityBuilder.Property(t => t.Active)
                .HasDefaultValue(true); // Valor padrão compatível

            entityBuilder.Property(t => t.AllowSendingEmail)
                .ValueGeneratedNever()
                .HasDefaultValue(true);

            // Removido LastLogin default - será definido no código

            entityBuilder.Property(t => t.ParentAproved);
            entityBuilder.Property(t => t.ParentEmail);
            entityBuilder.Property(t => t.ParentHashCodeAproval);
        }
    }
}
