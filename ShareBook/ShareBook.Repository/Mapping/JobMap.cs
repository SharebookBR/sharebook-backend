using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class JobMap
    {
        public JobMap(EntityTypeBuilder<Job> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.Name)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.Description)
                .HasColumnType("varchar(2000)")
                .HasMaxLength(2000)
                .IsRequired();
        }
    }

    public class JobHistoryMap
    {
        public JobHistoryMap(EntityTypeBuilder<JobHistory> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.LastResult)
                .HasColumnType("varchar(200)")
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.Details)
                .HasColumnType("varchar(2000)")
                .HasMaxLength(2000)
                .IsRequired();

        }
    }
}
