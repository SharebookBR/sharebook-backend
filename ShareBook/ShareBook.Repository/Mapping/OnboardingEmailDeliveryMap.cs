using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class OnboardingEmailDeliveryMap : IEntityTypeConfiguration<OnboardingEmailDelivery>
{
    public void Configure(EntityTypeBuilder<OnboardingEmailDelivery> entityBuilder)
    {
        entityBuilder.HasKey(delivery => delivery.Id);

        entityBuilder.HasOne(delivery => delivery.OnboardingJourney)
            .WithMany(journey => journey.Deliveries)
            .HasForeignKey(delivery => delivery.OnboardingJourneyId)
            .OnDelete(DeleteBehavior.Cascade);

        entityBuilder.HasIndex(delivery => new { delivery.OnboardingJourneyId, delivery.Step })
            .IsUnique();

        entityBuilder.HasIndex(delivery => new { delivery.QueuedAtUtc, delivery.Step });
    }
}
