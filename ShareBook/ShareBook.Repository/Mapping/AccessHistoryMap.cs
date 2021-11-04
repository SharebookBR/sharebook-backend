using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping  {
    public class AccessHistoryMap : IEntityTypeConfiguration<AccessHistory> {
        public void Configure(EntityTypeBuilder<AccessHistory> builder) {
            builder.ToTable("AccessHistories");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.VisitorName).HasMaxLength(200);
            builder.HasOne(a => a.User)
                .WithMany(u => u.Visitors)
                .HasForeignKey(a => a.UserId);

        }
    }
}