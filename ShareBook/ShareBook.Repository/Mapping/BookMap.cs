using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookMap
    {
        public BookMap(EntityTypeBuilder<Book> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.UserId);

            entityBuilder.Property(t => t.UserIdFacilitator);

            entityBuilder.Property(t => t.Author)
                .HasColumnType("varchar(200)")
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.Title)
                .HasColumnType("varchar(max)")
                .HasMaxLength(50)
                .IsRequired();

            entityBuilder.Property(t => t.ImageSlug)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsRequired();

            entityBuilder.Property(t => t.Slug)
               .HasColumnType("varchar(100)")
               .HasMaxLength(100);

            entityBuilder.Property(t => t.Synopsis)
               .HasColumnType("varchar(2000)")
               .HasMaxLength(2000);

            entityBuilder.Property(t => t.FacilitatorNotes)
               .HasColumnType("varchar(2000)")
               .HasMaxLength(2000);

            entityBuilder.Ignore(t => t.ImageBytes);

            entityBuilder.Ignore(t => t.ImageUrl);

            entityBuilder.Ignore(t => t.ImageName);

            entityBuilder.HasOne(t => t.User);

            entityBuilder.HasOne(t => t.UserFacilitator);

            entityBuilder.HasOne(t => t.Category);

            entityBuilder.Property(t => t.TrackingNumber)
                 .HasColumnType("varchar(max)");
        }
    }
}
