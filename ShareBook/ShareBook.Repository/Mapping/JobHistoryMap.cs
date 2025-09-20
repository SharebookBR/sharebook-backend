using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class JobHistoryMap : IEntityTypeConfiguration<JobHistory>
    {
        public void Configure(EntityTypeBuilder<JobHistory> entityBuilder)
        {
            entityBuilder.HasKey(t => t.Id);

            entityBuilder.Property(t => t.JobName)
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(t => t.LastResult)
                .HasMaxLength(200);

            entityBuilder.Property(t => t.Details)
                .HasMaxLength(4000); // Limite compatível com todos os bancos

        }
    }
}
