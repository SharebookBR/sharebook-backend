using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShareBook.Domain;

namespace ShareBook.Repository.Mapping {
    public class UserCancellationInfoMap : IEntityTypeConfiguration<UserCancellationInfo> {
        public void Configure(EntityTypeBuilder<UserCancellationInfo> builder) {
            builder.ToTable("UsersCancellationInfo");

            builder.HasKey(info => info.UserId);
            builder.Property(info => info.Reason).HasMaxLength(250).IsRequired();

            builder.HasOne(info => info.User).WithOne(user => user.UserCancellationInfo);
        }
    }
}