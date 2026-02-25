using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookMap : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.UserId);

            entityBuilder.Property(t => t.UserIdFacilitator);

            entityBuilder.Property(t => t.Author)
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.ImageSlug)
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.Slug)
               .HasMaxLength(100);

            entityBuilder.Property(t => t.Synopsis)
               .HasMaxLength(2000);

            entityBuilder.Property(t => t.FacilitatorNotes)
               .HasMaxLength(2000);

            entityBuilder.Ignore(t => t.ImageBytes);

            entityBuilder.Ignore(t => t.ImageUrl);

            entityBuilder.Ignore(t => t.ImageName);

            entityBuilder.Ignore(t => t.PdfBytes);

            entityBuilder.Property(t => t.EBookPdfPath)
                .HasMaxLength(500);

            entityBuilder.Property(t => t.Type)
                .HasConversion<int>();

            entityBuilder.HasOne(t => t.User);

            entityBuilder.HasOne(t => t.UserFacilitator);

            entityBuilder.HasOne(t => t.Category);
        }
    }
}
