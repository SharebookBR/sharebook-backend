using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class LegacyOnboardingBatchMap : IEntityTypeConfiguration<LegacyOnboardingBatch>
{
    public void Configure(EntityTypeBuilder<LegacyOnboardingBatch> entityBuilder)
    {
        entityBuilder.HasKey(batch => batch.Id);
        entityBuilder.HasIndex(batch => batch.SequenceNumber).IsUnique();
        entityBuilder.HasIndex(batch => batch.ExecutedAtUtc);
    }
}
