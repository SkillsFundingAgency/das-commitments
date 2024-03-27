using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class HistoryConfiguration : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.Property(e => e.ChangeType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.CreatedOn).HasColumnType("datetime");
            builder.Property(e => e.EntityType).HasMaxLength(50);
            builder.Property(e => e.UpdatedByName).HasMaxLength(255);

            builder.Property(e => e.UpdatedByRole)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Diff)
                .IsRequired(false);
        }
    }
}