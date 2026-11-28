using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class OnboardingJourneyMap : IEntityTypeConfiguration<OnboardingJourney>
{
    public void Configure(EntityTypeBuilder<OnboardingJourney> entityBuilder)
    {
        entityBuilder.HasKey(journey => journey.Id);

        entityBuilder.HasOne(journey => journey.User)
            .WithMany()
            .HasForeignKey(journey => journey.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entityBuilder.HasOne(journey => journey.LegacyOnboardingBatch)
            .WithMany(batch => batch.Journeys)
            .HasForeignKey(journey => journey.LegacyOnboardingBatchId)
            .OnDelete(DeleteBehavior.SetNull);

        // Uma conta nova só pode entrar uma vez na jornada.
        entityBuilder.HasIndex(journey => journey.UserId)
            .IsUnique();

        entityBuilder.HasIndex(journey => new { journey.CompletedAtUtc, journey.CanceledAtUtc, journey.NextEmailAtUtc });
        entityBuilder.HasIndex(journey => journey.LegacyOnboardingBatchId);

        entityBuilder.Property(journey => journey.CompletionReason)
            .HasMaxLength(120);
    }
}
