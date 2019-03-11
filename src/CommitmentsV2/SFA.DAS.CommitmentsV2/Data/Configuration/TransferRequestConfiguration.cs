using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.Configuration
{
    public class TransferRequestConfiguration : IEntityTypeConfiguration<TransferRequest>
    {
        public void Configure(EntityTypeBuilder<TransferRequest> builder)
        {
            builder.Property(e => e.Cost).HasColumnType("money");
            builder.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            builder.Property(e => e.FundingCap).HasColumnType("money");
            builder.Property(e => e.TrainingCourses).IsRequired();
            builder.Property(e => e.TransferApprovalActionedByEmployerEmail).HasMaxLength(255);
            builder.Property(e => e.TransferApprovalActionedByEmployerName).HasMaxLength(255);

            builder.HasOne(d => d.Commitment)
                .WithMany(p => p.TransferRequest)
                .HasForeignKey(d => d.CommitmentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}