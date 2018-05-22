using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain.Common;

namespace ShareBook.Repository.Mapping
{
    public class BaseEntityMap
    {
        public BaseEntityMap(EntityTypeBuilder<BaseEntity> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);
        }
    }
}
