using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class BookReleaseInterestMap : IEntityTypeConfiguration<BookReleaseInterest>
{
    public void Configure(EntityTypeBuilder<BookReleaseInterest> entityBuilder)
    {
        entityBuilder.HasKey(i => i.Id);

        entityBuilder.HasOne(i => i.Book)
            .WithMany()
            .HasForeignKey(i => i.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        entityBuilder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // No máximo um aviso por pessoa e livro. Garante idempotência mesmo sob concorrência.
        entityBuilder.HasIndex(i => new { i.BookId, i.UserId })
            .IsUnique();

        entityBuilder.HasIndex(i => i.NotifiedAt);
    }
}
