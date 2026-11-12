using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping;

public class OnboardingEmailClickMap : IEntityTypeConfiguration<OnboardingEmailClick>
{
    public void Configure(EntityTypeBuilder<OnboardingEmailClick> entityBuilder)
    {
        entityBuilder.HasKey(click => click.Id);

        entityBuilder.Property(click => click.Cta)
            .HasMaxLength(32);

        entityBuilder.HasOne(click => click.OnboardingEmailDelivery)
            .WithMany(delivery => delivery.Clicks)
            .HasForeignKey(click => click.OnboardingEmailDeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        entityBuilder.HasIndex(click => new { click.OnboardingEmailDeliveryId, click.Cta })
            .IsUnique();
    }
}
