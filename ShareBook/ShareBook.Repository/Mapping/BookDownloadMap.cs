using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookDownloadMap : IEntityTypeConfiguration<BookDownload>
    {
        public void Configure(EntityTypeBuilder<BookDownload> builder)
        {
            builder.ToTable("BookDownloads");

            builder.HasKey(bd => bd.Id);

            builder.Property(bd => bd.DownloadedAt)
                .IsRequired();

            builder.Property(bd => bd.UserAgent)
                .HasMaxLength(500);

            builder.Property(bd => bd.IpAddress)
                .HasMaxLength(45);

            builder.HasOne(bd => bd.Book)
                .WithMany()
                .HasForeignKey(bd => bd.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bd => bd.User)
                .WithMany()
                .HasForeignKey(bd => bd.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}