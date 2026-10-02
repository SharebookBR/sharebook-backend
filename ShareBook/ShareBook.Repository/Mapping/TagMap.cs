using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class TagMap : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> entityBuilder)
    {
        entityBuilder.HasKey(t => t.Id);

        entityBuilder.Property(t => t.Id)
            .HasMaxLength(100)
            .IsRequired();

        entityBuilder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        entityBuilder.Property(t => t.Aliases)
            .IsRequired();

        entityBuilder.Property(t => t.Family)
            .HasMaxLength(80)
            .IsRequired();

        entityBuilder.Property(t => t.Description)
            .HasMaxLength(500);

        entityBuilder.Property(t => t.UsageNotes)
            .HasMaxLength(1000);

        entityBuilder.Property(t => t.Status)
            .HasConversion<int>();

        entityBuilder.Property(t => t.IsPublic)
            .HasDefaultValue(true);

        entityBuilder.HasIndex(t => new { t.Status, t.IsPublic });

        entityBuilder.HasIndex(t => new { t.Family, t.Name });
    }
}
