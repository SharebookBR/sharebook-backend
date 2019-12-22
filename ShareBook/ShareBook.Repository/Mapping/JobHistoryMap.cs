using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class JobHistoryMap
    {
        public JobHistoryMap(EntityTypeBuilder<JobHistory> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.JobName)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.LastResult)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200);

            entityBuilder.Property(t => t.Details)
                .HasColumnType("varchar(1000)");
        }
    }
}