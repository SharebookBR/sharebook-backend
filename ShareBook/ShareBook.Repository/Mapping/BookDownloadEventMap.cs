using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookDownloadEventMap : IEntityTypeConfiguration<BookDownloadEvent>
    {
        public void Configure(EntityTypeBuilder<BookDownloadEvent> entityBuilder)
        {
            entityBuilder.HasKey(e => e.Id);

            entityBuilder
                .HasOne(e => e.Book)
                .WithMany()
                .HasForeignKey(e => e.BookId);

            entityBuilder
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired(false);

            entityBuilder
                .HasIndex(e => new { e.DownloadedAtUtc, e.BookId });

            entityBuilder
                .HasIndex(e => new { e.BookId, e.DownloadedAtUtc });

            entityBuilder
                .HasIndex(e => new { e.UserId, e.DownloadedAtUtc });
        }
    }
}
