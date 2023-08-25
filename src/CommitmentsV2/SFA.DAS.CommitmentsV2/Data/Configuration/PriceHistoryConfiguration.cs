using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
    {
        public void Configure(EntityTypeBuilder<PriceHistory> builder)
        {
            builder.Property(e => e.Cost).HasColumnType("decimal(18, 0)");
            builder.Property(e => e.TrainingPrice).HasColumnType("decimal(18, 0)");
            builder.Property(e => e.EndPointAssessmentPrice).HasColumnType("decimal(18, 0)");
            builder.Property(e => e.FromDate).HasColumnType("datetime");
            builder.Property(e => e.ToDate).HasColumnType("datetime");

            builder.HasOne(d => d.Apprenticeship)
                .WithMany(p => p.PriceHistory)
                .HasForeignKey(d => d.ApprenticeshipId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}