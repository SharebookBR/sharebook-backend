using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class MailBounceMap : IEntityTypeConfiguration<MailBounce>
    {
        public void Configure(EntityTypeBuilder<MailBounce> entityBuilder)
        {
            entityBuilder.HasIndex("Email");
        }
    }
}
