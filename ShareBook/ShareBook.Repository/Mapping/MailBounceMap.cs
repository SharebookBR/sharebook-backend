using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class MailBounceMap
    {
        public MailBounceMap(EntityTypeBuilder<MailBounce> entityBuilder)
        {
            entityBuilder.HasIndex("Email");
        }
    }
}
