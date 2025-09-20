using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookUserMap : IEntityTypeConfiguration<BookUser>
    {
        public void Configure(EntityTypeBuilder<BookUser> entityBuilder)
        {

            entityBuilder
             .HasKey(bu => new { bu.Id, bu.BookId, bu.UserId });

            entityBuilder
                .HasOne(bu => bu.Book)
                .WithMany(b => b.BookUsers)
                .HasForeignKey(bu => bu.BookId);

            entityBuilder
                 .HasOne(bu => bu.User)
                .WithMany(u => u.BookUsers)
                .HasForeignKey(bu => bu.UserId);

            entityBuilder.Property(bu => bu.Note)
               .HasMaxLength(2000);

            entityBuilder.Property(bu => bu.Reason)
                .HasMaxLength(2000);

            entityBuilder.Property(bu => bu.NickName)
                .HasMaxLength(64);
        }
    }
}
