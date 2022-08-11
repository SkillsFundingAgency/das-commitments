using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class OverlappingTrainingDateRequestConfiguration : IEntityTypeConfiguration<OverlappingTrainingDateRequest>
    {
        public void Configure(EntityTypeBuilder<OverlappingTrainingDateRequest> builder)
        {
            builder.ToTable("OverlappingTrainingDateRequest");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

            builder.HasOne(d => d.DraftApprenticeship)
                .WithMany(p => p.OverlappingTrainingDateRequests)
                .HasForeignKey(d => d.DraftApprenticeshipId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(d => d.PreviousApprenticeship)
                .WithMany(p => p.OverlappingTrainingDateRequests)
                .HasForeignKey(d => d.PreviousApprenticeshipId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
