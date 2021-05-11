using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class EbookComplaintMap
    {
        public EbookComplaintMap(EntityTypeBuilder<EbookComplaint> entityBuilder)
        {
            entityBuilder
             .HasKey(bu => new { bu.Id, bu.BookId, bu.UserId });

            entityBuilder.HasOne(t => t.User);
            entityBuilder.HasOne(t => t.Book);
            entityBuilder.Property(t => t.ReasonMessage)
                .HasColumnType("varchar(500)")
                .HasMaxLength(500);
        }
    }
}
