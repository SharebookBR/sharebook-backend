using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class BookTagMap : IEntityTypeConfiguration<BookTag>
{
    public void Configure(EntityTypeBuilder<BookTag> entityBuilder)
    {
        entityBuilder.HasKey(t => t.Id);

        entityBuilder.Property(t => t.TagId)
            .HasMaxLength(100)
            .IsRequired();

        entityBuilder.Property(t => t.Position)
            .IsRequired();

        entityBuilder.Property(t => t.Source)
            .HasConversion<int>();

        entityBuilder.Property(t => t.ReviewStatus)
            .HasConversion<int>();

        entityBuilder.HasOne(t => t.Book)
            .WithMany(t => t.BookTags)
            .HasForeignKey(t => t.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        entityBuilder.HasOne(t => t.Tag)
            .WithMany()
            .HasForeignKey(t => t.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        entityBuilder.HasIndex(t => new { t.BookId, t.TagId })
            .IsUnique();

        entityBuilder.HasIndex(t => new { t.BookId, t.Position })
            .IsUnique();

        entityBuilder.HasIndex(t => new { t.TagId, t.BookId });

        entityBuilder.ToTable(t => t.HasCheckConstraint("CK_BookTags_Position", "\"Position\" BETWEEN 1 AND 3"));
    }
}
